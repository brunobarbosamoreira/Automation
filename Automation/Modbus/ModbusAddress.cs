using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Automation.Modbus
{
    public struct Address
    {
        public Table Table { get; set; }
        public UInt16 Index { get; set; }
        public override String ToString()
        {
            return this.GetHashCode().ToString();
        }

        // overload operator !=
        public static Boolean operator !=(Address a, Address b)
        {
            return a.GetHashCode() != b.GetHashCode();
        }

        // overload operator ==
        public static Boolean operator ==(Address a, Address b)
        {
            return a.GetHashCode() == b.GetHashCode();
        }

        // overload operator <
        public static Boolean operator <(Address a, Address b)
        {
            return a.GetHashCode() < b.GetHashCode();
        }

        // overload operator <=
        public static Boolean operator <=(Address a, Address b)
        {
            return a.GetHashCode() <= b.GetHashCode();
        }

        // overload operator >
        public static Boolean operator >(Address a, Address b)
        {
            return a.GetHashCode() > b.GetHashCode();
        }

        // overload operator >=
        public static Boolean operator >=(Address a, Address b)
        {
            return a.GetHashCode() >= b.GetHashCode();
        }

        public override int GetHashCode()
        {
            int temp = (int)Table;
            if (Index > 9998)
            {
                temp *= 10;
            }
            temp += Index;
            temp++;
            return temp;
        }

        public override bool Equals(object obj)
        {
            if (obj is Address)
            {
                Address a = (Address)obj;
                return a == this;
            }
            return false;
        }
    }

    public enum Table : int
    {
        Coils = 0,
        DiscretesInput = 10000,
        InputRegisters = 30000,
        HoldingRegisters = 40000
    }
}
