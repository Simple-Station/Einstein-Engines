{
  pkgs ? (
    let
      lock = builtins.fromJSON (builtins.readFile ./flake.lock);
    in
    import (fetchTarball {
      url = "https://github.com/NixOS/nixpkgs/archive/${lock.nodes.nixpkgs.locked.rev}.tar.gz";
      sha256 = lock.nodes.nixpkgs.locked.narHash;
    }) { }
  ),
}:

let
  dependencies = with pkgs; [
    dotnet-sdk_9
    gcc14.cc.lib

    # Windowing
    sdl3
    glfw
    icu

    # Graphics
    mesa
    libGL
    libGLU
    libdrm
    egl-wayland
    wayland
    wayland-protocols
    libxkbcommon
    freetype
    openssl
    cacert


    # Audio
    openal
    alsa-lib
    alsa-plugins
    pipewire
    pulseaudio
    libvorbis
    fluidsynth
    soundfont-fluid

    # GTK stack
    gtk3
    pango
    cairo
    atk
    glib
    gdk-pixbuf

    # X11 / XWayland compatibility
    xorg.libxcb
    xorg.libX11
    xorg.libXcomposite
    xorg.libXdamage
    xorg.libXext
    xorg.libXcursor
    xorg.libXfixes
    xorg.libXrandr
    xorg.libxshmfence
    xorg.libXi

    # Misc runtime deps
    zlib
    nss
    nspr
    expat
    dbus
    at-spi2-atk
    at-spi2-core
  ];

  libraryPath = pkgs.lib.makeLibraryPath dependencies;
in
pkgs.mkShell {
  name = "space-station-14-devshell";

  packages = dependencies;

  shellHook = ''
    export GLIBC_TUNABLES=glibc.rtld.dynamic_sort=1

    # SDL: prefer native Wayland but allow XWayland fallback
    export SDL_VIDEODRIVER=x11

    # OpenAL: prefer PipeWire
    export ALSOFT_DRIVERS=pipewire,pulse,alsa

    # ALSA plugin discovery
    export ALSA_PLUGIN_DIR=${pkgs.alsa-plugins}/lib/alsa-lib

    # RobustToolbox soundfont
    export ROBUST_SOUNDFONT_OVERRIDE=${pkgs.soundfont-fluid}/share/soundfonts/FluidR3_GM2-2.sf2

    # GTK/GSettings
    export XDG_DATA_DIRS=$GSETTINGS_SCHEMAS_PATH

    # Graphics drivers from host system
    export LD_LIBRARY_PATH=${pkgs.openssl.out}/lib:${libraryPath}:/run/opengl-driver/lib

    export SSL_CERT_FILE=${pkgs.cacert}/etc/ssl/certs/ca-bundle.crt

    # Dotnet tools
    export PATH="$PATH:$HOME/.dotnet/tools"

    echo "SDL video drivers: $SDL_VIDEODRIVER"
    echo "OpenAL drivers: $ALSOFT_DRIVERS"
    echo ".NET SDK version: $(dotnet --version)"
  '';
}
