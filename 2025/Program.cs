using System.Diagnostics;
using System.Globalization;
using System.Numerics;
using System.Text;
using System.Text.RegularExpressions;

internal class Program {

	public class NumbersRange(long lo, long hi) {
		public long low = lo;
		public long high = hi;
		public bool ignored = false;

		public bool Contains(long n) {
			return n >= low && n <= high;
		}
	}

	#region Day 8 stuff

	public class Junction(long x, long y, long z) {
		public long x = x;
		public long y = y;
		public long z = z;

		static public long GetDistanceSquared(Junction a, Junction b) {
			long xDist = Math.Abs(a.x - b.x);
			long yDist = Math.Abs(a.y - b.y);
			long zDist = Math.Abs(a.z - b.z);

			xDist *= xDist;
			yDist *= yDist;
			zDist *= zDist;

			long totalDistanceSquared = xDist+yDist+zDist;
			return totalDistanceSquared; //Mathf.Sqrt(totalDistance);
		}

		public override string ToString() {
			return $"Junction({x},{y},{z})";
		}
	}
	
	public class Circuit {
		public List<Junction> boxes = new List<Junction>();

		public Circuit(Junction a) {
			boxes.Add(a);
		}

		public bool HasBox(Junction box) {
			foreach (Junction b in boxes) {
				if (b.x == box.x && b.y == box.y && b.z == box.z) {
					return true;
				}
			}
			return false;
		}
	}

	public class JunctionPair(Junction a, Junction b, long d) {
		public Junction boxA = a;
		public Junction boxB = b;
		public long distance = d;
	}

	#endregion

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
		if (Puzzle4("./04ex.pip") != 43) {
			throw new Exception("PUZZLE 4 EXAMPLE FAILED");
		}
		if (Puzzle7("./07ex.pip") != 40) {
			throw new Exception("PUZZLE 7 EXAMPLE FAILED");
		}
		DumpTime("END OF EXAMPLES. START OF REAL PUZZLES");
		Puzzle1();	// Combo lock puzzle
		Puzzle2();	// repeated digits serial id puzzle
		Puzzle3();	// Battery sequence puzzle 2 12
		Puzzle4();	// Removing carpet rolls puzzle
		Puzzle5();	// fresh food in ranges ID puzzle
		Puzzle6();	// vertical rtl math reading puzzle
		Puzzle7();	// tachyon christmas tree beam thing
		Puzzle8();	// Coordinates chaining

		static long Puzzle8(string fileName = "./08.pip", int matchesToMake = 1000) {
			DumpTime("P8S");
			string[] pip08 = File.ReadAllLines(fileName);

			List<Circuit> chains = new List<Circuit>();
			List<Junction> junctionBoxes = new List<Junction>();
			for (int i = 0; i < pip08.Length; i++) {
				string[] coords = pip08[i].Split(',');
				int x = int.Parse(coords[0]);
				int y = int.Parse(coords[1]);
				int z = int.Parse(coords[2]);
				Junction box = new Junction(x, y, z);
				junctionBoxes.Add(box);
				chains.Add(new Circuit(box));
			}
			// Findings not stated in example:
			// Junctions can connect to as many other junctions, a circuit doesn't need to be circuit but can also branch
			// Even if "nothing happens!", that still counts as making a connection
			List<JunctionPair> pairs = new List<JunctionPair>();

			for (int i = 0; i < junctionBoxes.Count; i++) {
				for (int j = i+1; j < junctionBoxes.Count; j++) {
					pairs.Add(new JunctionPair(junctionBoxes[i], junctionBoxes[j], Junction.GetDistanceSquared(junctionBoxes[i],junctionBoxes[j]))); 
				}
			}
			pairs = pairs.OrderBy(p => p.distance).ToList();
			
			int matchesMade = 0;

			DumpTime("Day 8 overhead done");
			for (int i = 0; i < pairs.Count; i++) {
				Junction boxA = pairs[i].boxA;
				Junction boxB = pairs[i].boxB;
				Circuit cA = chains.Find(c => c.HasBox(boxA));
				Circuit cB = chains.Find(c => c.HasBox(boxB));
				matchesMade++;
				if (cA != cB) {
					cA.boxes.AddRange(cB.boxes);
					chains.Remove(cB);
					Console.WriteLine($"Match {matchesMade} made: {boxA} & {boxB} -> {cA.boxes.Count} {chains.Count}");
				}
				if (matchesMade == matchesToMake) {
					chains = chains.OrderByDescending(c => c.boxes.Count).ToList();
					int password1 = chains[0].boxes.Count * chains[1].boxes.Count * chains[2].boxes.Count;
					DumpTime("PW1 found");
					Console.WriteLine("PW1: "+password1);
				}
				if (chains.Count == 1) {
					long password2 = boxA.x * boxB.x;
					DumpTime("PW2 found");
					Console.WriteLine("PW2: "+password2);
					break;
				}
			}
			return 0;
		}

		static long Puzzle7(string fileName = "./07.pip") {
			DumpTime("P7S");
			string[] pip07 = File.ReadAllLines(fileName);
			char[][] pip07w = new char[pip07.Length][];
			long[][] pip07i = new long[pip07.Length][];
			
			int password1 = 0;
			long password2 = 0;
			int startCol = pip07[0].IndexOf('S');
			int bottomRow = pip07.Length-1;
			pip07w[0] = pip07[0].ToCharArray();
			pip07i[0] = new long[pip07[0].Length];
			pip07i[0][startCol] = 1;
			int rowLength = pip07w[0].Length;
			for (int r = 1; r < pip07.Length; r++) {
				password2 = 0;
				pip07w[r] = pip07[r].ToCharArray();
				pip07i[r] = (long[])pip07i[r-1].Clone();
				for (int c = 0; c < rowLength; c++) {
					// if (r==bottomRow) {
					// 	continue;
					// }
					char aboveChar = pip07w[r-1][c];
					char thisChar = pip07w[r][c];
					long aboveInt = pip07i[r-1][c];
					if (thisChar == '^' && aboveChar == '|') {
						password1++;
						pip07w[r][c-1] = '|';
						pip07w[r][c+1] = '|';

						pip07i[r][c] = 0;
						pip07i[r][c-1] += aboveInt;
						pip07i[r][c+1] += aboveInt;
					}
					else if (aboveChar == '|' || aboveChar == 'S') {
						pip07w[r][c] = '|';
					}
					password2 += pip07i[r][c];
				}
				Console.WriteLine(new string(pip07w[r]) + " " + password2);
			}
			List<string> pop07 = new List<string>();
			foreach (char[] s in pip07w) {
				pop07.Add(new string(s));
			}
			
			File.WriteAllLines("./07.pop", pop07);
			Console.WriteLine("PW1:"+password1);
			Console.WriteLine("PW2:"+password2);
			// 1740473197 < ans

			return password2;
		}

		static long Puzzle6(string fileName = "./06.pip") {
			DumpTime("P6S");
			string[] pip06 = File.ReadAllLines(fileName);
			
			
			string[] operators = pip06[pip06.Length - 1].Split(new char[]{' '}, StringSplitOptions.RemoveEmptyEntries);
			long[] totalValueLtr = new long[operators.Length];
			long password1 = 0;

			for (int i = 0; i <= pip06.Length - 2; i++) {
				string[] vals = pip06[i].Split(new char[]{' '}, StringSplitOptions.RemoveEmptyEntries);
				for (int j = 0; j < vals.Length; j++) {
					if (i == 0) {
						totalValueLtr[j] = long.Parse(vals[j]);
					}
					else if (operators[j] == "*") {
						totalValueLtr[j] *= long.Parse(vals[j]);
					}
					else if (operators[j] == "+") {
						totalValueLtr[j] += long.Parse(vals[j]);
					}
					else {
						throw new Exception("Invalid operator");
					}
				}
			}
			foreach (long v in totalValueLtr) {
				password1 += v;
			}
			Console.WriteLine("PW1: " + password1);
			
			DumpTime("d6p2");
			// This basically converts the writing system to a western world human readable
			// format, so I could do an easier eyeball check on the results.
			// Then it takes those results and just does the math.

			List<string> pop06 = new List<string>();
			for (int c = pip06[0].Length-1; c >= 0; c--) {
				string l = "";
				for (int r = 0; r < pip06.Length; r++) {
					char ch = pip06[r][c];
					if (ch == ' ') {
						continue;
					}
					else if (ch == '*' || ch == '+') {
						pop06.Add(l);
						l = ch.ToString();
					}
					else {
						l += ch;
					}
				}
				pop06.Add(l);
			}
			File.WriteAllLines("./06.pop", pop06);
			
			long password2 = 0;
			List<long> bank = new List<long>();
			foreach (string line in pop06) {
				if (string.IsNullOrEmpty(line)) {
					continue;
				}
				else if (line == "*" || line == "+") {
					for (int i = 1; i < bank.Count; i++) {
						bank[0] = line[0]=='*' ? bank[0]*bank[i] : bank[0]+bank[i];
					}
					password2 += bank[0];
					bank.Clear();
				}
				else {
					bank.Add(long.Parse(line));
				}
			}
			Console.WriteLine("PW2: " + password2);

			DumpTime("d6p2 alternative method (better)");
			// My dumbass wrote this, then it spat out the wrong answer, and it took me until after I did
			// the whole thing over the above way that I realized, I had swapped + and * around in this.
			// So here, have both methods. 

			password2 = 0;
			char op = '?';
			string[] numbers = new string[pip06.Length - 1];
			Array.Fill(numbers, string.Empty);
			
			int totalColumns = pip06[0].Length;
			int operatorRow = pip06.Length - 1;
			int startingColumn = 0;

			for (int column = 0; column < totalColumns; column++) {
				char bottomRow = pip06[operatorRow][column];
				op = bottomRow == ' ' ? op : bottomRow;
				
				int emptyRows = 0;
				for (int row = 0; row < operatorRow; row++) {
					if (pip06[row][column] == ' ') {
						emptyRows++;
					}
					else {
						numbers[column - startingColumn] += pip06[row][column];
					}
					if (emptyRows == operatorRow || (column == totalColumns-1 && row == operatorRow-1)) {
						long formulaSolution = long.Parse(numbers[0]);
						for (int number = 1; number < numbers.Length; number++) {
							if (numbers[number] == "") {
								continue;
							}
							if (op == '*') {
								formulaSolution *= long.Parse(numbers[number]);
							}
							else if (op == '+') {
								formulaSolution += long.Parse(numbers[number]);
							}
						}
						password2 += formulaSolution;
						Array.Fill(numbers, string.Empty);
						startingColumn = column + 1;
						break;
					}
				}
			}
			Console.WriteLine("PW2: " + password2);
			DumpTime("d6E");

			return 0;
		}

		static long Puzzle5(string fileName = "./05.pip") {
			
			DumpTime("P5 read start");
			string[] pip05 = File.ReadAllLines(fileName);

			List<NumbersRange> ranges = new List<NumbersRange>();

			bool readingRanges = true;
			int password1 = 0;

			DumpTime("P5 A1 calc start");
			foreach (string line in pip05) {
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

			DumpTime("P5 sort start");
			ranges = ranges.OrderBy(r => r.low).ToList();
			DumpTime("P5 A2 start");
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

			DumpTime("P5 done");
			Console.WriteLine("PW1 = " + password1);
			Console.WriteLine("PW2 = " + password2);
			return 0;
		}

		static long Puzzle4(string fileName = "./04.pip") {
			DumpTime("P4 start");
			string[] pip04 = File.ReadAllLines(fileName);
			int rows = pip04[0].Length;
			int cols = pip04.Length;

			int password1 = rows * cols;
			int password2 = 0;
			
			bool[,] removedRolls = new bool[rows,cols];
			int emergency = 1000;
			bool firstPass = true;
			int rollsRemovedThisPass;

			// Holy nesting :)
			do {
				rollsRemovedThisPass = 0;
				emergency--;
				if (emergency <= 0) {
					break;
				}
				for (int row = 0; row < rows; row++) {
					for (int col = 0; col < cols; col++) {
						
						if (pip04[row][col] == '.') {
							password1--;
							continue;
						}
						if (removedRolls[row,col]) {
							continue;
						}
						int adjacentRolls = 0;
						for (int r = row-1; r <= row+1; r++) {
							for (int c = col-1; c <= col+1; c++) {
								if (r < 0 || c < 0) {
									continue;
								}
								if (r >= rows || c >= cols) {
									continue;
								}
								if (r == row && c == col) {
									continue;
								}
								bool isRoll = pip04[r][c] == '@';
								bool isRemoved = removedRolls[r,c];
								if (firstPass && isRoll) {
									adjacentRolls++;
									if (adjacentRolls == 4) {
										password1--;
									}
								}
								else if (isRoll && !isRemoved) {
									adjacentRolls++;
								}
							}
						}
						if (adjacentRolls < 4) {
							removedRolls[row,col] = true;
							rollsRemovedThisPass++;
						}
					}
				}
				if (firstPass) {
					Console.WriteLine("PW1=" + password1);
					DumpTime("PW1 calced");
					firstPass = false;
				}
				if (rollsRemovedThisPass == 0) {
					break;
				}
				password2 += rollsRemovedThisPass;
			}
			while (rollsRemovedThisPass > 0);

			Console.WriteLine("PW2=" + password2);
			DumpTime("PW2 calced");
			return password2;
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