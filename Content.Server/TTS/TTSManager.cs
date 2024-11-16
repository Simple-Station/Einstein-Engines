using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Content.Shared.CCVar;
using Prometheus;
using Robust.Shared.Configuration;
using Robust.Shared.ContentPack;
using Robust.Shared.Utility;

namespace Content.Server.TTS;

// ReSharper disable once InconsistentNaming
public sealed class TTSManager
{
    private static readonly Histogram RequestTimings = Metrics.CreateHistogram(
        "tts_req_timings",
        "Timings of TTS API requests",
        new HistogramConfiguration()
        {
            LabelNames = new[] {"type"},
            Buckets = Histogram.ExponentialBuckets(.1, 1.5, 10),
        });

    private static readonly Counter WantedCount = Metrics.CreateCounter(
        "tts_wanted_count",
        "Amount of wanted TTS audio.");

    private static readonly Counter ReusedCount = Metrics.CreateCounter(
        "tts_reused_count",
        "Amount of reused TTS audio from cache.");

    [Dependency] private readonly IConfigurationManager _cfg = default!;
    [Dependency] private readonly IResourceManager _resource = default!;
    private ISawmill _sawmill = default!;

    private readonly Dictionary<int, byte[]> _memoryCache = new();
    private ResPath _cachePath = new();
    private ResPath _modelPath = new();

    public TTSManager()
    {
        Initialize();
    }

    private void Initialize()
    {
        IoCManager.InjectDependencies(this);
        _sawmill = Logger.GetSawmill("tts");

        _cachePath = MakeDataPath(_cfg.GetCVar(CCVars.TTSCachePath));
        _cfg.OnValueChanged(CCVars.TTSCachePath, OnCachePathChanged);
        _modelPath = MakeDataPath(_cfg.GetCVar(CCVars.TTSModelPath));
        _cfg.OnValueChanged(CCVars.TTSModelPath, OnModelPathChanged);

        // Make the needed directories if they don't exist
        new Process
        {
            StartInfo = new ProcessStartInfo
            {
                #if WINDOWS
                FileName = "cmd.exe",
                Arguments = $"/C \"mkdir {_cachePath} {_modelPath}\"",
                #else
                FileName = "/bin/sh",
                Arguments = $"-c \"mkdir -p {_cachePath} {_modelPath}\"",
                #endif
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = true,
            },
        }.Start();
    }

    private void OnCachePathChanged(string path)
        => _cachePath = MakeDataPath(path);
    private void OnModelPathChanged(string path)
        => _modelPath = MakeDataPath(path);

    private ResPath MakeDataPath(string path)
    {
        if (path.StartsWith("data/"))
            return new(_resource.UserData.RootDir + path.Remove(0, 5));
        else
            return new(path); // Hope it's valid
    }


    /// <summary>
    ///     Generates audio with passed text by API
    /// </summary>
    /// <param name="model">File name for the model</param>
    /// <param name="speaker">Identifier of speaker</param>
    /// <param name="text">SSML formatted text</param>
    /// <returns>OGG audio bytes or null if failed</returns>
    public async Task<byte[]?> ConvertTextToSpeech(string model, string speaker, string text)
    {
        WantedCount.Inc();

        var key = $"{model}/{speaker}/{text}".GetHashCode();
        var file = await TryGetCached(key);
        if (file != null)
        {
            ReusedCount.Inc();
            return file;
        }

        var fileName = _cachePath + ResPath.SystemSeparatorStr + key + ".wav";
        var strCmdText = $"echo '{text}' | piper --model {(_modelPath + ResPath.SystemSeparatorStr + model)}.onnx --speaker {speaker} --output_file {fileName}";

        var proc = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                #if WINDOWS
                FileName = "cmd.exe",
                Arguments = $"/C \"{strCmdText}\"",
                #else
                FileName = "/bin/sh",
                Arguments = $"-c \"{strCmdText}\"",
                #endif
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = true,
            },
        };
        var reqTime = DateTime.UtcNow;
        try
        {
            proc.Start();
            await proc.WaitForExitAsync().ConfigureAwait(false);
        }
        catch (Exception e)
        {
            RequestTimings.WithLabels("Error").Observe((DateTime.UtcNow - reqTime).TotalSeconds);
            _sawmill.Error($"Failed of request generation new sound for '{text}' speech by '{speaker}' speaker\n{e}");
            return null;
        }

        file = await File.ReadAllBytesAsync(fileName);
        TryCache(key, file);
        return file;
    }

    private bool TryCache(int key, byte[] file)
    {
        // Delete extra files
        if (_cfg.GetCVar(CCVars.TTSCacheType) != "memory")
        {
            var files = Directory.GetFiles(_cachePath + ResPath.SystemSeparatorStr).ToList()
                .OrderBy(f => File.GetLastWriteTimeUtc(f).Ticks);
            var count = files.Count();
            var toDelete = count - _cfg.GetCVar(CCVars.TTSMaxCached);
            for (var i = toDelete; i > 0; i--)
                File.Delete(files.ElementAt(i));
            return false;
        }
        while (_memoryCache.Count > _cfg.GetCVar(CCVars.TTSMaxCached))
            _memoryCache.Remove(_memoryCache.First().Key);

        // Cache to memory
        File.Delete(_cachePath + ResPath.SystemSeparatorStr + key + ".wav");
        return _memoryCache.TryAdd(key, file);
    }

    /// Tries to find an existing audio file so we don't have to make another
    private async Task<byte[]?> TryGetCached(int key)
    {
        var type = _cfg.GetCVar(CCVars.TTSCacheType);
        switch (type)
        {
            case "file":
                var path = _cachePath + ResPath.SystemSeparatorStr + key + ".wav";
                return !File.Exists(path) ? null : await File.ReadAllBytesAsync(path);
            case "memory":
                return _memoryCache.GetValueOrDefault(key);
            default:
                DebugTools.Assert(false, "TTSCacheType is invalid, must be one of \"file\", \"memory\"");
                return null;
        }
    }

    /// Deletes every file with the .wav extension in the _cachePath and clears the memory cache
    public void ClearCache()
    {
        new Process
        {
            StartInfo = new ProcessStartInfo
            {
                #if WINDOWS
                FileName = "cmd.exe",
                Arguments = $"/C \"del /q {_cachePath}\\*.wav\"",
                #else
                FileName = "/bin/sh",
                Arguments = $"-c \"rm {_cachePath}/*.wav\"",
                #endif
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = true,
            },
        }.Start();
        _memoryCache.Clear();
    }
}
