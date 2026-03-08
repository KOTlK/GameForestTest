using System.Diagnostics;
using System.Text;
using System.Runtime.CompilerServices;

public static class Assertions {
    private static StringBuilder sb = new();

    public class FUCKYOUCSHARP {}
    [Conditional("DEBUG")]
    public static void Assert(bool          expr,
                              string        errorMessage = "",
                              FUCKYOUCSHARP sorry         = null,
           [CallerFilePath]   string        filePath     = "",
           [CallerLineNumber] int           lineNumber   = 0) {
        if(!expr) {
            Console.WriteLine($"Assert failed at {filePath}:{lineNumber}. {errorMessage}");
        }
    }

    [Conditional("DEBUG")]
    public static void Assert(bool            expr,
                              string          errorMessage,
                              params object[] args) {
        if(!expr) {
            sb.Clear();

            var stackTrace  = new StackTrace(true);
            var frame       = stackTrace.GetFrame(1);
            var fileName    = frame.GetFileName();
            var lineNum     = frame.GetFileLineNumber();

            sb.Append("Assert failed at ");
            sb.Append(fileName);
            sb.Append(':');
            sb.Append(lineNum);
            sb.Append(". ");

            var len = errorMessage.Length;
            var cur = 0;

            for (var i = 0; i < len; i++) {
                switch(errorMessage[i]) {
                    case '%' :
                        if (cur >= args.Length) {
                            Console.WriteLine("Number of arguments provided to a format function does not match.");
                            sb.Clear();
                            return;
                        }
                        sb.Append(args[cur++].ToString());
                    break;
                    default :
                        sb.Append(errorMessage[i]);
                    break;
                }
            }

            Console.WriteLine(sb.ToString());

            sb.Clear();
        }
    }
}