using System.Reflection;
using System.Collections.Generic;

namespace CashRegisterRepairs.Utilities.Helpers
{
    public static class FieldValidator
    {
        private static List<char> allowedSpecialSymbols = new List<char>()
        {
            '.','"','&','(',')','-','+',' '
        };

        public static bool HasAnEmptyField(object businessObject)
        {
            foreach (PropertyInfo pi in businessObject.GetType().GetProperties())
            {
                if (pi.PropertyType == typeof(string))
                {
                    string value = (string)pi.GetValue(businessObject);
                    if (string.IsNullOrEmpty(value))
                    {
                        return true;
                    }
                }
            }

            return false;
        }


        public static bool HasAnIncorrectlyFormattedField(object businessObject)
        {
            foreach (PropertyInfo pi in businessObject.GetType().GetProperties())
            {
                if (pi.PropertyType == typeof(string))
                {
                    string value = (string)pi.GetValue(businessObject);
                    foreach (char c in value)
                    {
                        if (!char.IsLetterOrDigit(c) && !allowedSpecialSymbols.Contains(c))
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }
    }
}