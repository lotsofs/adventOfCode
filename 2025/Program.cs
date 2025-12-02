using System.Diagnostics;
using System.Text.RegularExpressions;

internal class Program {
    private static readonly Stopwatch stopwatch = Stopwatch.StartNew();
    private static long lastTick = 0;

    private static void Main(string[] args) {
        DumpTime("START");
        if (Puzzle2("./02ex.pip") != 1227775554) {
            throw new Exception("PUZZLE 2 EXAMPLE FAILED");
        }
        DumpTime("END OF EXAMPLES. START OF REAL PUZZLES");
        Puzzle1();
        Puzzle2();

        static long Puzzle2(string fileName = "./02.pip") {
            DumpTime("Puzzle 2 start read");
            string pip02 = File.ReadAllText(fileName);

            DumpTime("Puzzle 2 start split");
            string[] ranges = pip02.Split(',');
            long password1 = 0;
            long password2 = 0;

            DumpTime("Puzzle 2 start process");
            for (int i = 0; i < ranges.Length; i++) {
                // This will not work if the range spans multiple factors of 100,
                // Eg.: 1234-345678, but these are not present in the data. Sad :(
                string[] bounds = ranges[i].Split('-');
                long effectiveLowerBound = long.Parse(bounds[0]);
                long actualLowerBound = effectiveLowerBound;
                long upperBound = long.Parse(bounds[1]);
                int digitCount = effectiveLowerBound.ToString().Length;
                if (digitCount % 2 == 1) {
                    effectiveLowerBound = (long)Math.Pow(10, digitCount);
                    digitCount += 1;
                }

                long halfPow = (long)Math.Pow(10, digitCount / 2);
                long halfDigits = effectiveLowerBound / halfPow;

                for (long j = halfDigits; j < halfPow; j++) {
                    long invalidNumber = j * halfPow + j;
                    if (invalidNumber > upperBound)
                    {
                        break;
                    }
                    if (invalidNumber < actualLowerBound)
                    {
                        continue;
                    }
                    password1 += invalidNumber;
                }

                // Strings D: Math much more fun
                Regex r = new Regex(@"^(\w+)\1+$");
                for (long k = actualLowerBound; k < upperBound; k++) {
                    Match m = r.Match(k.ToString());
                    if (m.Success) {
                        password2 += long.Parse(m.Value);
                    }
                }
            }
            DumpTime("Puzzle 2 start print");
            Console.WriteLine("password1: " + password1);
            Console.WriteLine("password2: "+password2);
            return password1;
        }

        static void Puzzle1() {
            DumpTime("Start of puzzle 1 read");
            string[] pip01 = File.ReadAllLines("./01.pip");
            int currentPosition = 50;
            int password1 = 0;
            int password2 = 0;

            DumpTime("Start of puzzle 1 process");
            // Dumbly do the rotations. Tried doing something fancy
            // but having to deal with all the edge cases just made it
            // ugly but with no significant bonus satisfaction.
            for (int i = 0; i < pip01.Length; i++) {
                string p = pip01[i];
                char direction = p[0];
                int count = int.Parse(p[1..]);

                for (int j = 0; j < count; j++)
                {
                    if (direction == 'L') {
                        currentPosition--;
                    }
                    else {
                        currentPosition++;
                    }
                    if (currentPosition % 100 == 0) {
                        password2++;
                        if (j == count - 1) {
                            password1++;
                        }
                    }
                }
            }
            DumpTime("End of puzzle 1");
            Console.WriteLine("password1: " + password1);
            Console.WriteLine("password2: " + password2);
        }
        
        static void DumpTime(string log = "") {
            long current = stopwatch.ElapsedTicks;
            double ticksPerMs = Stopwatch.Frequency / 1000;
            double totalMs = (double)current / ticksPerMs;
            double deltaMs = lastTick == 0 ? totalMs : (double)(current-lastTick) / ticksPerMs;
            Console.WriteLine("");
            Console.WriteLine($"Elapsed time since start: {totalMs:0.###} ms, since last dump: {deltaMs:0.###} ms");
            Console.WriteLine($"========{log}========");
            lastTick = current;
        }
    }
}