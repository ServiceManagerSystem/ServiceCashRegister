using System.Reflection;

namespace CashRegisterRepairs.Utilities.Helpers
{
    public static class FieldValidator
    {
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


        // And so on
    }
}
