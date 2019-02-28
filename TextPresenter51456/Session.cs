using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TextPresenter51456 {
    class Session {
        private static long id;
        private static long state;

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

        public static void New() {
            id = DateTime.Now.Ticks;
            state = 0;
            Console.WriteLine("id: {0} / state: {1}", id, state);
        }

        public static void IncreaseState() {
            state += 1;
            Console.WriteLine("id: {0} / state: {1}", id, state);
        }

    }
}
