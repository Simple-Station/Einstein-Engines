using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;

namespace Spessembly
{
    public class PLCInterpreter
    {
        private const char pinPrefix = 'p';
        private const char registerPrefix = 'r';
        private const char immediatePrefix = '#';
        private const char addressStart = '[';
        private const char addressEnd = ']';

        public const int HIGH = 100;
        public const int LOW = 0;

        private string[] code = Array.Empty<string>();
        private int[] pins;
        private int[] registers;
        private int[] memory;
        public string Name;

        private int CLK = 0, POWER = 0, prevCLK = 0;

        private Dictionary<string, int> labels = new Dictionary<string, int>();
        private bool syntaxOk = false;

        public int AmountOfPins => pins.Length;
        public int AmountOfRegisters => registers.Length;
        public int MemorySize => memory.Length;

        private int executionIndex = 0;
        private bool awaitingClk = false;
        private bool deleted = false;

        private static Thread executorThread = new Thread(ExecutionLoop);
        private static List<PLCInterpreter> allPlcs = new List<PLCInterpreter>();
        private static bool executing;

        public bool IsPowered => POWER >= 50;

        private static void ExecutionLoop()
        {
            while (executing)
            {
                // Ensure chips aren't removed from the list during the foreach(chip)
                allPlcs.RemoveAll(c => c.deleted);

                foreach (PLCInterpreter plc in allPlcs)
                {
                    plc.TryPerformStep();
                }
            }
        }

        public bool IsBusy => !FinishedExecution && IsPowered;
        public bool FinishedExecution => executionIndex == code.Length;

        public static void Initialize()
        {
            executing = true;
            executorThread.Start();
        }

        public static void Deinitialize()
        {
            executing = false;
            executorThread.Abort();
            allPlcs = new List<PLCInterpreter>();
        }

        public PLCInterpreter(string name, int amountOfPins, int amountOfRegisters, int memoryCapacity)
        {
            Name = name;
            pins = new int[amountOfPins];
            registers = new int[amountOfRegisters];
            memory = new int[memoryCapacity];
            allPlcs.Add(this);
        }

        public void Destroy()
        {
            deleted = true;
        }

        public void LoadCode(string[] code)
        {
            this.code = code;

            syntaxOk = CheckSyntax();

            if (!syntaxOk)
            {
                ConsolePrint("Syntax error");
                return;
            }
        }

        public void SetPin(int pin, int value)
        {
            pins[pin] = value;
        }

        public int GetPin(int pin)
        {
            return pins[pin];
        }

        public void SetCLK(int value)
        {
            CLK = value;
        }

        public int GetCLK()
        {
            return CLK;
        }

        public void SetPOWER(int value) //should not be higher than 100, but it's really just an on/off
        {
            if (value < 50) // on too low power...
            {
                executionIndex = 0;

                for (int i = 0, l = registers.Length; i < l; i++)
                {
                    registers[i] = 0;
                }
            }

            POWER = value;
        }

        public int GetPOWER()
        {
            return POWER;
        }

        public void SetRegister(int register, int value)
        {
            registers[register] = value;
        }

        public int GetRegister(int register)
        {
            return registers[register];
        }

        public int[] DumpMemory()
        {
            int[] copy = new int[memory.Length];
            memory.CopyTo(copy, 0);
            return copy;
        }

        private void TryPerformStep()
        {
            try
            {
                Step();
            }
            catch (Exception e)
            {
                ConsolePrint("Error occurred during execution");
                ConsolePrint(e.ToString());
            }
        }

        private void Step()
        {
            if (!syntaxOk)
            {
                ConsolePrint("Cannot execute; fix syntax errors first.");
                return;
            }

            if ((CLK >= 50 && prevCLK < 50) || POWER < 50)
                awaitingClk = false;

            prevCLK = CLK;

            if (executionIndex == code.Length || awaitingClk || POWER < 50 || deleted)
                return;

            string line = code[executionIndex].Trim().Split(';')[0].ToLower(); //remove trailing spaces and ignore comments

            if (code[executionIndex].Contains(";"))
            {
                string comment = code[executionIndex].Trim().Split(';')[1];

                ConsolePrint(comment.Trim());
            }

            executionIndex++;

            if (line.Length == 0 || line[0] == ':') //ignore blank lines and labels
                return;

            line = Regex.Replace(line, " *, +", ","); //paste arguments together for convenience

            string[] linePrts = line.Split(' ');

            string instruction = linePrts[0];
            string[] args = linePrts.Length > 1 ? line.Split(' ')[1].Split(',') : new string[0];

            int value;

            switch (instruction)
            {
                case "print":
                    value = GetValueFromSource(args[1]);
                    ConsolePrint($"PRINT '{value}'");
                    break;

                case "mov":
                    value = GetValueFromSource(args[1]);
                    SetValueIntoDest(args[0], value);
                    break;

                case "store":

                    value = GetValueFromSource(args[1]);

                    if (args[0].First() == addressStart && args[0].Last() == addressEnd)
                    {
                        string src = Regex.Replace(args[0], @"[\[\]]", "");
                        int address = GetNumberFromSource(src, true);
                        memory[address] = value;
                    }
                    else
                    {
                        throw new InvalidOperationException($"'{args[0]}' is not a memory address");
                    }
                    break;

                case "load":

                    if (args[1].First() == addressStart && args[1].Last() == addressEnd)
                    {
                        string src = Regex.Replace(args[1], @"[\[\]]", "");
                        int address = GetNumberFromSource(src, true);
                        value = memory[address];
                    }
                    else
                    {
                        throw new InvalidOperationException($"'{args[1]}' is not a memory address");
                    }

                    SetValueIntoDest(args[0], value);

                    break;

                case "add":

                    value = (byte) (GetNumberFromSource(args[1]) + GetNumberFromSource(args[2]));
                    SetValueIntoDest(args[0], value);

                    break;

                case "sub":

                    value = (byte) (GetNumberFromSource(args[1]) - GetNumberFromSource(args[2]));
                    SetValueIntoDest(args[0], value);

                    break;

                case "mul":

                    value = (byte) (GetNumberFromSource(args[1]) * GetNumberFromSource(args[2]));
                    SetValueIntoDest(args[0], value);

                    break;

                case "div":

                    value = (byte) (GetNumberFromSource(args[1]) / GetNumberFromSource(args[2]));
                    SetValueIntoDest(args[0], value);

                    break;

                case "mod":

                    value = (byte) (GetNumberFromSource(args[1]) % GetNumberFromSource(args[2]));
                    SetValueIntoDest(args[0], value);

                    break;

                case "and":

                    value = (byte) (GetValueFromSource(args[1]) & GetValueFromSource(args[2]));
                    SetValueIntoDest(args[0], value);

                    break;

                case "or":

                    value = (byte) (GetValueFromSource(args[1]) | GetValueFromSource(args[2]));
                    SetValueIntoDest(args[0], value);

                    break;

                case "not":

                    value = (byte) ~GetValueFromSource(args[1]);
                    SetValueIntoDest(args[0], value);

                    break;

                case "jmp":
                    executionIndex = labels[args[0]];
                    break;

                case "jz":
                    if (GetValueFromSource(args[0]) == 0)
                        executionIndex = labels[args[1]];
                    break;

                case "jnz":
                    if (GetValueFromSource(args[0]) != 0)
                        executionIndex = labels[args[1]];
                    break;

                case "clk":
                    awaitingClk = true;
                    break;

                case "cmpgt":

                    value = (byte) (GetValueFromSource(args[1]) > GetValueFromSource(args[2]) ? 1 : 0);
                    SetValueIntoDest(args[0], value);

                    break;

                case "cmplt":

                    value = (byte) (GetValueFromSource(args[1]) < GetValueFromSource(args[2]) ? 1 : 0);
                    SetValueIntoDest(args[0], value);

                    break;

                case "cmpeq":

                    value = (byte) (GetValueFromSource(args[1]) == GetValueFromSource(args[2]) ? 1 : 0);
                    SetValueIntoDest(args[0], value);

                    break;
            }
        }

        private int GetNumberFromSource(string source, bool literalAllowed = false)
        {
            if (source[0] == pinPrefix)
            {
                return GetPin(int.Parse(source.Substring(1)));
            }
            else if (source[0] == registerPrefix)
            {
                return GetRegister(int.Parse(source.Substring(1)));
            }
            else if (source[0] == immediatePrefix)
            {
                return int.Parse(source.Substring(1));
            }
            else
            {
                if (literalAllowed)
                {
                    return int.Parse(source);
                }

                throw new InvalidOperationException($"Unknown source type in parameter '{source}'");
            }
        }

        private int GetValueFromSource(string source)
        {
            return (int) GetNumberFromSource((string) source, false);
        }

        private void SetValueIntoDest(string dest, int value)
        {
            if (dest[0] == pinPrefix)
            {
                SetPin(int.Parse(dest.Substring(1)), value);
            }
            else if (dest[0] == registerPrefix)
            {
                SetRegister(int.Parse(dest.Substring(1)), value);
            }
            else if (dest[0] == immediatePrefix)
            {
                throw new InvalidOperationException($"Cannot set a value to immediate value '{dest}'");
            }
            else
            {
                throw new InvalidOperationException($"Unknown source type in parameter '{dest}'");
            }
        }

        private bool CheckSyntax()
        {
            labels = new Dictionary<string, int>();
            try
            {
                for (int i = 0, l = code.Length; i < l; i++)
                {
                    string line = code[i].Trim().Split(';')[0]; //remove trailing spaces and ignore comments

                    if (line.Length == 0)
                        continue;

                    if (line[0] == ':')
                    {
                        labels[line.Split(' ')[0].Substring(1).ToLower()] = i;
                    }
                }
            }
            catch (Exception e)
            {
                ConsolePrint(e.Message);
                return false;
            }

            //todo: actually check syntax

            return true;
        }

        private void ConsolePrint(string text)
        {
            //TODO: This should go into a GUI, not the actual console.
            Console.WriteLine($"[{Name}]: {text}");
        }
    }
}
