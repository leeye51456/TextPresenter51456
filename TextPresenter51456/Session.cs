using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TextPresenter51456 {
    class Session {
        private static long id;
        private static long state;

        public static string Pgm { get; set; }
        public static string Pvw { get; set; }
        public static string Next { get; set; }

        public static long Id {
            get {
                return id;
            }
        }

        public static long State {
            get {
                return state;
            }
        }

        public static void New(string newPgm, string newPvw, string newNext) {
            id = DateTime.Now.Ticks;
            state = 0;
            Pgm = newPgm;
            Pvw = newPvw;
            Next = newNext;
            Console.WriteLine("id: {0} / state: {1}", id, state);
        }

        public static void IncreaseState(string newPgm, string newPvw, string newNext) {
            if (Pgm.Equals(newPgm) && Pvw.Equals(newPvw)) {
                Console.WriteLine("id: {0} / state: {1}", id, state);
                return;
            }
            state += 1;
            Pgm = newPgm;
            Pvw = newPvw;
            Next = newNext;
            Console.WriteLine("id: {0} / state: {1}", id, state);
        }

    }
}
