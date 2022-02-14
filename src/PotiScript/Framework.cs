using System;
using System.Linq;
using System.Threading.Tasks;

using PotiScript.Exceptions;
using PotiScript.Runtime;

namespace PotiScript
{
    public class Framework
    {
        public static void InstallGlobals(PotiScriptInterpreter interpreter)
        {
            interpreter.Add("today").Function((call, ct) =>
            {
                call.Return.DateTime(DateTime.UtcNow);
                return Task.CompletedTask;
            });

            interpreter.Add("print").Function((call, ct) =>
            {
                var text = call.Args[0].String() ?? string.Empty;
                interpreter.PrintLine(text);
                return Task.CompletedTask;
            });

            interpreter.Add("convert").Object(builder =>
            {
                builder.Add("toString").Function((call, ct) =>
                {
                    var result = Convert.ToString(call.Args.FirstOrDefault()?.Primitive());
                    if (result != null)
                    {
                        call.Return.String(result);
                    }
                    return Task.CompletedTask;
                });
                builder.Add("toNumber").Function((call, ct) =>
                {
                    call.Return.Number(Convert.ToDecimal(call.Args.FirstOrDefault()?.Primitive()));
                    return Task.CompletedTask;
                });
                builder.Add("toBoolean").Function((call, ct) =>
                {
                    call.Return.Boolean(Convert.ToBoolean(call.Args.FirstOrDefault()?.Primitive()));
                    return Task.CompletedTask;
                });
                builder.Add("toDateTime").Function((call, ct) =>
                {
                    call.Return.DateTime(Convert.ToDateTime(call.Args.FirstOrDefault()?.Primitive()));
                    return Task.CompletedTask;
                });
            });

            interpreter.Add("string").Object((builder) =>
            {
                builder.Add("getDigits").Function((call, ct) =>
                {
                    var inputString = call.Args.FirstOrDefault()?.String();

                    if (inputString != null)
                    {
                        call.Return.String(string.Join("", inputString.Where(char.IsDigit).ToArray()));
                    }

                    return Task.CompletedTask;
                });

                builder.Add("format").Function((call, ct) =>
                {
                    var formatString = call.Args.FirstOrDefault()?.String();

                    if (formatString != null)
                    {
                        var objects = call.Args.Skip(1).Select(x => x.Primitive()).ToArray();
                        call.Return.String(string.Format(formatString, objects));
                    }

                    return Task.CompletedTask;
                });
            });
        }

        public static void InstallExtensions(TypeSystem.Object @object, Func<string, ProxyWriter> add)
        {
        }

        public static void InstallExtensions(TypeSystem.DateTime @object, Func<string, ProxyWriter> add)
        {
            add("addDays").Function((call, ct) =>
            {
                var days = call.Args.FirstOrDefault()?.Number();
                if (days == null)
                {
                    call.Return.DateTime(@object.Value);
                }
                else
                {
                    call.Return.DateTime(@object.Value.AddDays((double)days.Value));
                }
                return Task.CompletedTask;
            });
        }

        public static void InstallExtensions(TypeSystem.String @object, Func<string, ProxyWriter> add)
        {
            add("length").Number(@object.Value.Length);

            add("substring").Function((call, ct) =>
            {
                var startIndex = call.Args.FirstOrDefault()?.Number();
                var length = call.Args.Skip(1).FirstOrDefault()?.Number();

                if (startIndex != null && length != null)
                {
                    call.Return.String(@object.Value.Substring(Convert.ToInt32(startIndex), Convert.ToInt32(length)));
                }
                else if (startIndex != null)
                {
                    call.Return.String(@object.Value.Substring(Convert.ToInt32(startIndex)));

                }
                else
                {
                    throw new RuntimeErrorException("substring requires a start index as a number.");
                }


                return Task.CompletedTask;
            });

            add("lastIndexOf").Function((call, ct) =>
            {
                var value = call.Args.FirstOrDefault()?.String();
                if (value != null)
                {
                    call.Return.Number(@object.Value.LastIndexOf(value));
                }
                return Task.CompletedTask;
            });
        }
    }
}
