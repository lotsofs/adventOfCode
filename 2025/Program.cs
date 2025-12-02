Puzzle1();

static void Puzzle1() {
    string[] pip01 = File.ReadAllLines("./01.pip");
    int currentPosition = 50;
    int password1 = 0;
    int password2 = 0;
    
    DateTime start = DateTime.Now;
    for (int i = 0; i < pip01.Length; i++) {
        string p = pip01[i];
        char direction = p[0];
        int count = int.Parse(p[1..]);

        for (int j = 0; j < count; j++) {
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
    DateTime end = DateTime.Now;
    TimeSpan span = end - start;
    double ms = span.TotalMilliseconds;
    Console.WriteLine("password1: "+password1);
    Console.WriteLine("password2: "+password2);
    Console.WriteLine("Done, took "+ms+" ms");
}