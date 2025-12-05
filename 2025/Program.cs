using System.Diagnostics;
using System.Globalization;
using System.Text.RegularExpressions;

internal class Program {

    public class NumbersRange(long lo, long hi)
    {
        public long low = lo;
        public long high = hi;
        public bool ignored = false;

        public bool Contains(long n) {
            return n >= low && n <= high;
        }
    }
    
    private static readonly Stopwatch stopwatch = Stopwatch.StartNew();
    private static long lastTick = 0;

    private static void Main(string[] args) {
        DumpTime("START");
        if (Puzzle2("./02ex.pip") != 1227775554) {
            throw new Exception("PUZZLE 2 EXAMPLE FAILED");
        }
        if (Puzzle3("./03ex.pip") != 3121910778619) {
            throw new Exception("PUZZLE 3 EXAMPLE FAILED");
        }
        DumpTime("END OF EXAMPLES. START OF REAL PUZZLES");
        Puzzle1();
        Puzzle2();
        Puzzle3();
        Puzzle4();

        static long Puzzle4(string fileName = "./04.pip") {
            
            DumpTime("P4 read start");
            string[] pip04 = File.ReadAllLines(fileName);

            List<NumbersRange> ranges = new List<NumbersRange>();

            bool readingRanges = true;
            int password1 = 0;

            DumpTime("P4 A1 calc start");
            foreach (string line in pip04) {
                if (string.IsNullOrEmpty(line)) {
                    readingRanges = false;
                    continue;
                }
                if (!readingRanges) {
                    long num = long.Parse(line);
                    for (int i = 0; i < ranges.Count; i++) {
                        if (ranges[i].Contains(num)) {
                            password1++;
                            break;
                        }
                    }
                    continue;
                }
                string[] halves = line.Split('-');
                long low = long.Parse(halves[0]);
                long high = long.Parse(halves[1]);
                
                ranges.Add(new NumbersRange(low, high));
            }

            DumpTime("P4 sort start");
            ranges = ranges.OrderBy(r => r.low).ToList();
            DumpTime("P4 A2 start");
            long password2 = 0;

            for (int j = 0; j < ranges.Count; j++) {
                NumbersRange workingRange = ranges[j];
                if (workingRange.ignored) {
                    continue;
                }
                for (int i = j+1; i < ranges.Count; i++) {
                    NumbersRange comparingRange = ranges[i];
                    if (comparingRange.low > workingRange.high) {
                        password2 += workingRange.high - workingRange.low + 1;
                        break;
                    }
                    workingRange.high = Math.Max(workingRange.high, comparingRange.high);
                    comparingRange.ignored = true;
                    if (i == ranges.Count - 1) {
                        password2 += workingRange.high - workingRange.low + 1;
                    }
                }
            }
            foreach (NumbersRange r in ranges) {
                Console.WriteLine($"{r.low:N0}  -  {r.high:N0} " + (r.ignored ? "" : "REAL"));
            }

            DumpTime("P4 done");
            Console.WriteLine("PW1 = " + password1);
            Console.WriteLine("PW2 = " + password2);
            return 0;
        }

        static long Puzzle3(string fileName = "./03.pip") {
            DumpTime("Puzzle 3 read start now");
            string[] pip03 = File.ReadAllLines(fileName);

            char x = '0';
            int password1 = 0;
            long password2 = 0;

            DumpTime("Puzzle 3 calc start now");
            foreach (string bank in pip03) {
                int battery1 = x;
                int battery2 = x;    
                for (int i = 0; i < bank.Length; i++) {
                    int battery = bank[i];
                    if (i < bank.Length - 1 && battery > battery1) {
                        battery1 = battery;
                        battery2 = x;
                    }
                    else if (battery > battery2) {
                        battery2 = battery;
                    }
                }
                battery1 -= x;
                battery1 *= 10;
                battery2 -= x;
                password1 += battery1 + battery2;
            }
            Console.WriteLine("password1: " + password1);

            DumpTime("Puzzle 3 start phase 2");
            foreach (string bank in pip03) {
                List<int> batterySequence = new List<int>();
                for (int i = bank.Length - 12; i <= bank.Length - 1; i++) {
                    batterySequence.Add(bank[i]-x);
                }

                char searchTarget = '9';
                int minIndex = 0;
                int maxIndex = bank.Length - 12;
                int index = minIndex;
                int emergency = 200*200*12;
                int establishedBatteries = 0;
                while (establishedBatteries < 12) {
                    emergency--;
                    if (emergency < 0) {
                        break;
                    }
                    if (searchTarget < batterySequence[establishedBatteries]) {
                        break;
                    }
                    int currentChar = bank[index] - x;
                    if (currentChar == searchTarget) {
                        batterySequence[establishedBatteries] = currentChar;
                        establishedBatteries++;
                        index++;
                        minIndex = index;
                        maxIndex++;
                        continue;
                    }    
                    if (index == maxIndex) {
                        searchTarget--;
                        index = minIndex;
                        continue;
                    }
                    index++;
                }
                long batSeq = 0;
                for (int i = 0; i < batterySequence.Count; i++) {
                    batSeq += batterySequence[i] * (long)Math.Pow(10, 11-i);
                }
                password2 += batSeq;
            }

            DumpTime("day 3 end");
            Console.WriteLine("password2: " + password2);
            return password2;
        }

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