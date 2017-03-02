using ExigoWeb.Kendo;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Common.Kendo
{
    public static class KendoUtilitiesCustom
    {
        public static string GetSorting(this KendoGridRequest request)
        {
            var expression = "";

            foreach (var sortObject in request.SortObjects)
            {
                expression += sortObject.Field + " " + sortObject.Direction + ", ";
            }

            if (expression.Length < 2)
                return "true";

            expression = expression.Substring(0, expression.Length - 2);

            return expression;
        }
        public static string GetFiltering<T>(this KendoGridRequest request)
        {
            var finalExpression = "";

            foreach (var filterObject in request.FilterObjectWrapper.FilterObjects)
            {
                if (finalExpression.Length > 0)
                    finalExpression += " " + request.FilterObjectWrapper.LogicToken + " ";


                if (filterObject.IsConjugate)
                {
                    var expression1 = GetExpression<T>(filterObject.Field1, filterObject.Operator1, filterObject.Value1);
                    var expression2 = GetExpression<T>(filterObject.Field2, filterObject.Operator2, filterObject.Value2);
                    var combined = string.Format("({0} {1} {2})", expression1, request.FilterObjectWrapper.LogicToken, expression2);
                    finalExpression += combined;
                }
                else
                {
                    var expression = GetExpression<T>(filterObject.Field1, filterObject.Operator1, filterObject.Value1);
                    finalExpression += expression;
                }
            }

            if (finalExpression.Length == 0)
                return "true";

            return finalExpression;
        }

        private static string GetExpression<T>(string field, string op, string param)
        {
            var p = typeof(T).GetProperty(field);

            var dataType = (p.PropertyType.IsGenericType && p.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>)) ?
                p.PropertyType.GetGenericArguments()[0].Name.ToLower() : p.PropertyType.Name.ToLower();


            var caseMod = "";
            if (dataType == "string")
            {
                param = @"""" + param.ToLower() + @"""";
                caseMod = ".ToLower()";
            }

            if (dataType == "datetime")
            {
                var i = param.IndexOf("GMT", StringComparison.Ordinal);
                if (i > 0)
                {
                    param = param.Remove(i);
                }
                var date = DateTime.Parse(param, new CultureInfo("en-US"));

                var str = string.Format("DateTime({0}, {1}, {2})", date.Year, date.Month, date.Day);
                param = str;
            }

            switch (op)
            {
                case "eq":
                    return string.Format("{0}{2} == {1}", field, param, caseMod);

                case "neq":
                    return string.Format("{0}{2} != {1}", field, param, caseMod);

                case "contains":
                    return string.Format("{0}{2}.Contains({1})", field, param, caseMod);

                case "startswith":
                    return string.Format("{0}{2}.StartsWith({1})", field, param, caseMod);

                case "endswith":
                    return string.Format("{0}{2}.EndsWith({1})", field, param, caseMod);

                case "gte":
                    return string.Format("{0}{2} >= {1}", field, param, caseMod);

                case "gt":
                    return string.Format("{0}{2} > {1}", field, param, caseMod);

                case "lte":
                    return string.Format("{0}{2} <= {1}", field, param, caseMod);

                case "lt":
                    return string.Format("{0}{2} < {1}", field, param, caseMod);

                default:
                    return "";
            }
        }



        #region SQL Helpers
        public static string GetSqlWhereClause(IEnumerable<FilterObject> filters)
        {
            var whereClauseFilters = new List<string>();
            foreach (var filter in filters)
            {
                filter.Field1 = filter.Field1.Replace("_", ".");

                whereClauseFilters.Add(GetSqlWhereClauseExpression(filter.Field1, filter.Operator1, filter.Value1));
            }
            var whereClause = string.Join(" AND ", whereClauseFilters);



            if (whereClauseFilters.Count > 0)
            {
                whereClause = " AND " + whereClause;
            }
            else
            {
                whereClause = "";
            };

            return whereClause;
        }
        public static string GetSqlWhereClauseExpression(string field, string op, string param)
        {
            var fieldFormat = "{0}";

            DateTime paramAsDateTime;
            if (DateTime.TryParse(param, out paramAsDateTime))
            {
                fieldFormat = " CAST({0} as date)";
            }

            var result = " ";

            switch (op)
            {
                case "eq":
                    result += string.Format(fieldFormat + " = '{1}' ", field, param); break;

                case "neq":
                    result += string.Format(fieldFormat + " <> '{1}' ", field, param); break;

                case "contains":
                    result += string.Format(fieldFormat + " LIKE '%{1}%' ", field, param); break;

                case "startswith":
                    result += string.Format(fieldFormat + " LIKE '{1}%' ", field, param); break;

                case "endswith":
                    result += string.Format(fieldFormat + " LIKE '%{1}' ", field, param); break;

                case "gte":
                    result += string.Format(fieldFormat + " >= '{1}' ", field, param); break;

                case "gt":
                    result += string.Format(fieldFormat + " > '{1}' ", field, param); break;

                case "lte":
                    result += string.Format(fieldFormat + " <= '{1}' ", field, param); break;

                case "lt":
                    result += string.Format(fieldFormat + " < '{1}' ", field, param); break;
            }


            return result;
        }

        public static string GetSqlOrderByClause(IEnumerable<SortObject> filters, params SortObject[] defaultFilters)
        {
            var orderbyClauseFilters = new List<string>();
            foreach (var sort in filters)
            {
                sort.Field = sort.Field.Replace("_", ".");
                orderbyClauseFilters.Add("{0} {1}".FormatWith(sort.Field, sort.Direction));
            }

            if ((defaultFilters != null && defaultFilters.Length > 0) && !filters.Any(c => defaultFilters.Select(x => x.Field.Trim()).Contains(c.Field.Trim(), StringComparer.InvariantCultureIgnoreCase)))
            {
                foreach (var defaultFilter in defaultFilters)
                {
                    orderbyClauseFilters.Add("{0} {1}".FormatWith(defaultFilter.Field, defaultFilter.Direction));
                }
            }


            var orderbyClause = string.Join(", ", orderbyClauseFilters);

            return orderbyClause;
        }
        #endregion
    }
}