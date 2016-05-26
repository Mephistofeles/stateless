using System;

namespace BugTrackerExample
{
    public static class Program
    {
        public static void Main()
        {
            var bug = new Bug("Incorrect stock count");

            bug.Assign("Joe");
            bug.Defer();
            bug.Assign("Harry");
            bug.Assign("Fred");
            bug.Close();

            Console.ReadKey(false);
        }
    }
}
