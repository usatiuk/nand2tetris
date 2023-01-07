using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Security;
using System.Linq;

namespace JackAnalyzer
{
    class Writer
    {
        private Queue<KeyValuePair<TokenType, string>> tokenQueue;
        private static MetaSymbolTable MetaSymbolTable;
        VMWriter VMWriter;
        string ClassName;
        int LabelCounter = 0;
        public Writer(string path, List<KeyValuePair<TokenType, string>> tokens)
        {
            if(MetaSymbolTable == null)
            {
                MetaSymbolTable = new MetaSymbolTable();
            } else
            {
                MetaSymbolTable.ResetFieldTable();
            }
            VMWriter = new VMWriter(path);
            string filename = Path.ChangeExtension(path, ".xml");
            ClassName = Path.GetFileNameWithoutExtension(filename);
            tokenQueue = new Queue<KeyValuePair<TokenType, string>>(tokens);
            using (StreamWriter file = new StreamWriter(filename))
            {
                var token = tokenQueue.Peek();

                if (token.Key == TokenType.keyword)
                {
                    string keyword = token.Value;
                    if (keyword == "class")
                    {
                        string[] classLines = CompileClass();
                        foreach (string line in classLines)
                        {
                            file.WriteLine(line);
                        }
                    }
                }
            }
            VMWriter.Dispose();
        }

        // 'class' className '{' classVarDec* subroutineDec* '}'
        private string[] CompileClass()
        {
            List<string> output = new List<string>();

            output.Add(PutOpenTag(0, "class"));

            output.Add(PutToken(1));
            var token = tokenQueue.Dequeue();
            output.Add(PutLineTag(1, Enum.GetName(typeof(TokenType), token.Key), token.Value, "class, defined"));
            output.Add(PutToken(1));

            // classVarDec*
            token = tokenQueue.Peek();
            while (token.Value == "static" || token.Value == "field")
            {
                output.AddRange(CompileClassVarDec(1));
                token = tokenQueue.Peek();
            }

            // subroutineDec*
            token = tokenQueue.Peek();
            while (token.Value == "function" || token.Value == "constructor" || token.Value == "method" || token.Value == "void")
            {
                output.AddRange(CompileSubroutine(1));
                token = tokenQueue.Peek();
            }

            // } 
            output.Add(PutToken(1));

            output.Add(PutCloseTag(0, "class"));

            return output.ToArray();
        }

        // ('static' | 'field') type varName (',' varName)* ';'
        private string[] CompileClassVarDec(int indent)
        {
            List<string> output = new List<string>();

            output.Add(PutOpenTag(indent, "classVarDec"));

            var token = tokenQueue.Dequeue();
            SymbolKind kind;
            switch (token.Value)
            {
                case "field":
                    kind = SymbolKind.Field;
                    break;
                case "static":
                    kind = SymbolKind.Static;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            output.Add(PutLineTag(indent + 1, Enum.GetName(typeof(TokenType), token.Key), token.Value));

            token = tokenQueue.Dequeue();
            string type = token.Value;
            output.Add(PutLineTag(indent + 1, Enum.GetName(typeof(TokenType), token.Key), token.Value));

            token = tokenQueue.Dequeue();
            string name = token.Value;
            MetaSymbolTable.Define(name, type, kind);
            output.Add(PutLineTag(indent + 1, Enum.GetName(typeof(TokenType), token.Key), token.Value, "name=" + name + " " + "type=" + type + " " + "kind=" + Enum.GetName(typeof(SymbolKind), kind) + " " + "index=" + MetaSymbolTable.IndexOf(name) + " " + "declared"));


            token = tokenQueue.Peek();
            while (token.Value == ",")
            {
                output.Add(PutToken(indent + 1));
                token = tokenQueue.Dequeue();
                name = token.Value;
                MetaSymbolTable.Define(name, type, kind);
                output.Add(PutLineTag(indent + 1, Enum.GetName(typeof(TokenType), token.Key), token.Value, "name=" + name + " " + "type=" + type + " " + "kind=" + Enum.GetName(typeof(SymbolKind), kind) + " " + "index=" + MetaSymbolTable.IndexOf(name) + " " + "declared"));
                token = tokenQueue.Peek();
            }

            output.Add(PutToken(indent + 1));

            output.Add(PutCloseTag(indent, "classVarDec"));

            return output.ToArray();
        }

        // ('constructor' | 'function' | 'method')
        // ('void' | type) subroutineName '(' parameterList ')' subroutineBody
        private string[] CompileSubroutine(int indent)
        {
            List<string> output = new List<string>();

            output.Add(PutOpenTag(indent, "subroutineDec"));
            MetaSymbolTable.ResetFunctionTable();
            var token = tokenQueue.Dequeue();
            string type = token.Value;
            output.Add(PutToken(indent + 1));
            token = tokenQueue.Dequeue();
            string name = token.Value;
            output.Add(PutLineTag(indent + 1, Enum.GetName(typeof(TokenType), token.Key), token.Value, "subroutine, defined"));
            output.Add(PutToken(indent + 1));

            // parameterList
            if(type == "method")
            {
                MetaSymbolTable.Define("this", ClassName, SymbolKind.Arg);
            }
            output.AddRange(CompileParameterList(indent + 1));
            VMWriter.WriteFunction(ClassName + "." + name, MetaSymbolTable.VarCount(SymbolKind.Var));
            output.Add(PutToken(indent + 1));
            switch(type)
            {
                case "constructor":
                    VMWriter.WritePush(Segment.Constant, MetaSymbolTable.VarCount(SymbolKind.Field));
                    VMWriter.WriteCall("Memory.alloc", 1);
                    VMWriter.WritePop(Segment.Pointer, 0);
                    break;
                case "function":
                    break;
                case "method":
                    VMWriter.WritePush(Segment.Argument, 0);
                    VMWriter.WritePop(Segment.Pointer, 0);
                    break;
            }
            output.AddRange(CompileSubroutineBody(indent + 1));
            VMWriter.RewriteFunctionLocals(ClassName + "." + name, MetaSymbolTable.VarCount(SymbolKind.Var));
            output.Add(PutCloseTag(indent, "subroutineDec"));

            return output.ToArray();
        }

        // '{' varDec* statements '}'
        private string[] CompileSubroutineBody(int indent)
        {
            List<string> output = new List<string>();

            output.Add(PutOpenTag(indent, "subroutineBody"));

            output.Add(PutToken(indent + 1));

            // varDec*
            var token = tokenQueue.Peek();
            while (token.Value == "var")
            {
                output.AddRange(CompileVarDec(indent + 1));
                token = tokenQueue.Peek();
            }

            output.AddRange(CompileStatements(indent + 1));

            // } 
            output.Add(PutToken(indent + 1));

            output.Add(PutCloseTag(indent, "subroutineBody"));

            return output.ToArray();
        }

        // statement*
        private string[] CompileStatements(int indent)
        {
            List<string> output = new List<string>();

            output.Add(PutOpenTag(indent, "statements"));

            bool finished = false;

            var token = tokenQueue.Peek();

            while (!finished)
            {
                switch (token.Value)
                {
                    case "do":
                        output.AddRange(CompileDoStatement(indent + 1));
                        break;
                    case "let":
                        output.AddRange(CompileLetStatement(indent + 1));
                        break;
                    case "while":
                        output.AddRange(CompileWhileStatement(indent + 1));
                        break;
                    case "return":
                        output.AddRange(CompileReturnStatement(indent + 1));
                        break;
                    case "if":
                        output.AddRange(CompileIfStatement(indent + 1));
                        break;
                    default:
                        finished = true;
                        break;
                }
                token = tokenQueue.Peek();
            }


            output.Add(PutCloseTag(indent, "statements"));

            return output.ToArray();
        }

        // 'do' subroutineCall ';'
        private string[] CompileDoStatement(int indent)
        {
            List<string> output = new List<string>();

            output.Add(PutOpenTag(indent, "doStatement"));

            output.Add(PutToken(indent + 1));

            var queueEnumerator = tokenQueue.GetEnumerator();
            queueEnumerator.MoveNext();
            queueEnumerator.MoveNext();
            var nextToken = queueEnumerator.Current;
            string name = "";

            int ellen = 0;

            // subroutineName '(' expressionList ')' | (className |
            // varName) '.' subroutineName '(' expressionList ')'
            if (nextToken.Value == ".")
            {

                var token = tokenQueue.Dequeue();
                string classname = token.Value;
                output.Add(PutLineTag(indent + 1, Enum.GetName(typeof(TokenType), token.Key), token.Value, "class, used"));
                output.Add(PutToken(indent + 1));
                token = tokenQueue.Dequeue();
                string fname = token.Value;
                output.Add(PutLineTag(indent + 1, Enum.GetName(typeof(TokenType), token.Key), token.Value, "function, used"));
                output.Add(PutToken(indent + 1));
                try
                {
                    MetaSymbolTable.KindOf(classname);
                    Segment seg = Segment.Local;
                    switch (MetaSymbolTable.KindOf(classname))
                    {
                        case SymbolKind.Static:
                            seg = Segment.Static;
                            break;
                        case SymbolKind.Field:
                            seg = Segment.This;
                            break;
                        case SymbolKind.Arg:
                            seg = Segment.Argument;
                            break;
                        case SymbolKind.Var:
                            seg = Segment.Local;
                            break;
                        case SymbolKind.None:
                            break;
                        default:
                            break;
                    }
                    VMWriter.WritePush(seg, MetaSymbolTable.IndexOf(classname));
                    ellen += 1;
                }
                catch
                {

                }

                ellen += CompileExpressionList(indent + 1);
                output.Add(PutToken(indent + 1));

                try
                {
                    name = MetaSymbolTable.TypeOf(classname) + "." + fname;

                } catch
                {
                    name = classname + "." + fname;
                }
            }
            else if (nextToken.Value == "(")
            {
                VMWriter.WritePush(Segment.Pointer, 0);
                var token = tokenQueue.Dequeue();
                name = ClassName + "." + token.Value;
                output.Add(PutLineTag(indent + 1, Enum.GetName(typeof(TokenType), token.Key), token.Value, "function, used"));
                output.Add(PutToken(indent + 1));
                ellen += CompileExpressionList(indent + 1);
                ellen += 1;
                output.Add(PutToken(indent + 1));
            }
            // ;
            output.Add(PutToken(indent + 1));

            VMWriter.WriteCall(name, ellen);
            VMWriter.WritePop(Segment.Temp, 0);
            output.Add(PutCloseTag(indent, "doStatement"));

            return output.ToArray();
        }

        // 'let' varName ('[' expression ']')? '=' expression ';'
        private string[] CompileLetStatement(int indent)
        {
            List<string> output = new List<string>();

            output.Add(PutOpenTag(indent, "letStatement"));

            output.Add(PutToken(indent + 1));
            var token = tokenQueue.Dequeue();
            string name = token.Value;
            output.Add(PutLineTag(indent + 1, Enum.GetName(typeof(TokenType), token.Key), token.Value, "name=" + name + " " + "type=" + MetaSymbolTable.TypeOf(name) + " " + "kind=" + Enum.GetName(typeof(SymbolKind), MetaSymbolTable.KindOf(name)) + " " + "index=" + MetaSymbolTable.IndexOf(name) + " " + "used"));

            token = tokenQueue.Peek();
            if (token.Value == "[")
            {
                output.Add(PutToken(indent + 1));
                Segment sega = Segment.Local;
                switch (MetaSymbolTable.KindOf(name))
                {
                    case SymbolKind.Static:
                        sega = Segment.Static;
                        break;
                    case SymbolKind.Field:
                        sega = Segment.This;
                        break;
                    case SymbolKind.Arg:
                        sega = Segment.Argument;
                        break;
                    case SymbolKind.Var:
                        sega = Segment.Local;
                        break;
                    case SymbolKind.None:
                        break;
                    default:
                        break;
                }
                VMWriter.WritePush(sega, MetaSymbolTable.IndexOf(name));
                output.AddRange(CompileExpression(indent + 1));
                VMWriter.WriteArithmetic(ArithmeticOp.Add);
                output.Add(PutToken(indent + 1));

                output.Add(PutToken(indent + 1));
                output.AddRange(CompileExpression(indent + 1));
                output.Add(PutToken(indent + 1));

                VMWriter.WritePop(Segment.Temp, 0);
                VMWriter.WritePop(Segment.Pointer, 1);
                VMWriter.WritePush(Segment.Temp, 0);
                VMWriter.WritePop(Segment.That, 0);

            }
            else
            {
                output.Add(PutToken(indent + 1));
                output.AddRange(CompileExpression(indent + 1));
                output.Add(PutToken(indent + 1));

                Segment seg = Segment.Local;
                switch (MetaSymbolTable.KindOf(name))
                {
                    case SymbolKind.Static:
                        seg = Segment.Static;
                        break;
                    case SymbolKind.Field:
                        seg = Segment.This;
                        break;
                    case SymbolKind.Arg:
                        seg = Segment.Argument;
                        break;
                    case SymbolKind.Var:
                        seg = Segment.Local;
                        break;
                    case SymbolKind.None:
                        break;
                    default:
                        break;
                }
                VMWriter.WritePop(seg, MetaSymbolTable.IndexOf(name));
            }

            output.Add(PutCloseTag(indent, "letStatement"));

            return output.ToArray();
        }

        // 'while' '(' expression ')' '{' statements '}'
        private string[] CompileWhileStatement(int indent)
        {
            List<string> output = new List<string>();
            LabelCounter++;
            int localcounter = LabelCounter;
            output.Add(PutOpenTag(indent, "whileStatement"));
            VMWriter.WriteLabel("WHILE" + localcounter + 0);
            output.Add(PutToken(indent + 1));
            output.Add(PutToken(indent + 1));
            output.AddRange(CompileExpression(indent + 1));
            VMWriter.WriteArithmetic(ArithmeticOp.Not);
            VMWriter.WriteIf("WHILE" + localcounter + 1);
            output.Add(PutToken(indent + 1));
            output.Add(PutToken(indent + 1));
            output.AddRange(CompileStatements(indent + 1));
            VMWriter.WriteGoto("WHILE" + localcounter + 0);
            output.Add(PutToken(indent + 1));
            VMWriter.WriteLabel("WHILE" + localcounter + 1);

            output.Add(PutCloseTag(indent, "whileStatement"));

            return output.ToArray();
        }

        // 'return' expression? ';'
        private string[] CompileReturnStatement(int indent)
        {
            List<string> output = new List<string>();

            output.Add(PutOpenTag(indent, "returnStatement"));

            output.Add(PutToken(indent + 1));

            var token = tokenQueue.Peek();
            if (token.Value != ";")
            {
                output.AddRange(CompileExpression(indent + 1));
            } else
            {
                VMWriter.WritePush(Segment.Constant, 0);
            }

            output.Add(PutToken(indent + 1));

            VMWriter.WriteReturn();
            output.Add(PutCloseTag(indent, "returnStatement"));

            return output.ToArray();
        }

        // 'if' '(' expression ')' '{' statements '}' ('else' '{' statements '}')?
        private string[] CompileIfStatement(int indent)
        {
            List<string> output = new List<string>();
            LabelCounter++;
            int localcounter = LabelCounter;

            output.Add(PutOpenTag(indent, "ifStatement"));

            output.Add(PutToken(indent + 1));
            output.Add(PutToken(indent + 1));
            output.AddRange(CompileExpression(indent + 1));
            VMWriter.WriteArithmetic(ArithmeticOp.Not);
            VMWriter.WriteIf("IF" + localcounter + 0);

            output.Add(PutToken(indent + 1));
            output.Add(PutToken(indent + 1));
            output.AddRange(CompileStatements(indent + 1));
            output.Add(PutToken(indent + 1));
            VMWriter.WriteGoto("IF" + localcounter + 1);
            VMWriter.WriteLabel("IF" + localcounter + 0);
            var token = tokenQueue.Peek();
            if (token.Value == "else")
            {
                output.Add(PutToken(indent + 1));
                output.Add(PutToken(indent + 1));
                output.AddRange(CompileStatements(indent + 1));
                output.Add(PutToken(indent + 1));
            }
            VMWriter.WriteLabel("IF" + localcounter + 1);

            output.Add(PutCloseTag(indent, "ifStatement"));

            return output.ToArray();
        }

        // 'var' type varName (',' varName)* ';'
        private string[] CompileVarDec(int indent)
        {
            List<string> output = new List<string>();

            output.Add(PutOpenTag(indent, "varDec"));

            var token = tokenQueue.Dequeue();
            SymbolKind kind = SymbolKind.Var;
            output.Add(PutLineTag(indent + 1, Enum.GetName(typeof(TokenType), token.Key), token.Value));

            token = tokenQueue.Dequeue();
            string type = token.Value;
            output.Add(PutLineTag(indent + 1, Enum.GetName(typeof(TokenType), token.Key), token.Value));

            token = tokenQueue.Dequeue();
            string name = token.Value;
            MetaSymbolTable.Define(name, type, kind);
            output.Add(PutLineTag(indent + 1, Enum.GetName(typeof(TokenType), token.Key), token.Value, "name=" + name + " " + "type=" + type + " " + "kind=" + Enum.GetName(typeof(SymbolKind), kind) + " " + "index=" + MetaSymbolTable.IndexOf(name) + " " + "declared"));

            token = tokenQueue.Peek();
            while (token.Value == ",")
            {
                output.Add(PutToken(indent + 1));

                token = tokenQueue.Dequeue();
                name = token.Value;
                MetaSymbolTable.Define(name, type, kind);
                output.Add(PutLineTag(indent + 1, Enum.GetName(typeof(TokenType), token.Key), token.Value, "name=" + name + " " + "type=" + type + " " + "kind=" + Enum.GetName(typeof(SymbolKind), kind) + " " + "index=" + MetaSymbolTable.IndexOf(name) + " " + "declared"));
                token = tokenQueue.Peek();
            }

            output.Add(PutToken(indent + 1));

            output.Add(PutCloseTag(indent, "varDec"));

            return output.ToArray();
        }

        // ((type varName) (',' type varName)*)?
        private string[] CompileParameterList(int indent)
        {
            List<string> output = new List<string>();

            output.Add(PutOpenTag(indent, "parameterList"));

            var token = tokenQueue.Peek();
            if (token.Key == TokenType.keyword || token.Key == TokenType.identifier)
            {
                token = tokenQueue.Dequeue();
                string type = token.Value;
                output.Add(PutLineTag(indent + 1, Enum.GetName(typeof(TokenType), token.Key), token.Value));

                token = tokenQueue.Dequeue();
                string name = token.Value;
                SymbolKind kind = SymbolKind.Arg;
                MetaSymbolTable.Define(name, type, kind);
                output.Add(PutLineTag(indent + 1, Enum.GetName(typeof(TokenType), token.Key), token.Value, "name=" + name + " " + "type=" + type + " " + "kind=" + Enum.GetName(typeof(SymbolKind), kind) + " " + "index=" + MetaSymbolTable.IndexOf(name) + " " + "declared"));

                token = tokenQueue.Peek();
                while (token.Value == ",")
                {
                    output.Add(PutToken(indent + 1));
                    token = tokenQueue.Dequeue();
                    type = token.Value;
                    output.Add(PutLineTag(indent + 1, Enum.GetName(typeof(TokenType), token.Key), token.Value));

                    token = tokenQueue.Dequeue();
                    name = token.Value;
                    kind = SymbolKind.Arg;
                    MetaSymbolTable.Define(name, type, kind);
                    output.Add(PutLineTag(indent + 1, Enum.GetName(typeof(TokenType), token.Key), token.Value, "name=" + name + " " + "type=" + type + " " + "kind=" + Enum.GetName(typeof(SymbolKind), kind) + " " + "index=" + MetaSymbolTable.IndexOf(name) + " " + "declared"));
                    token = tokenQueue.Peek();
                }
            }

            output.Add(PutCloseTag(indent, "parameterList"));

            return output.ToArray();
        }

        // term (op term)*
        private string[] CompileExpression(int indent)
        {
            List<string> output = new List<string>();

            output.Add(PutOpenTag(indent, "expression"));

            output.AddRange(CompileTerm(indent + 1));

            var token = tokenQueue.Peek();
            while (Constants.op.Contains(token.Value[0]))
            {
                string op = token.Value;
                output.Add(PutToken(indent + 1));
                output.AddRange(CompileTerm(indent + 1));
                switch (op)
                {
                    case "+":
                        VMWriter.WriteArithmetic(ArithmeticOp.Add);
                        break;
                    case "-":
                        VMWriter.WriteArithmetic(ArithmeticOp.Sub);
                        break;
                    case "*":
                        VMWriter.WriteCall("Math.multiply", 2);
                        break;
                    case "/":
                        VMWriter.WriteCall("Math.divide", 2);
                        break;
                    case "&":
                        VMWriter.WriteArithmetic(ArithmeticOp.And);
                        break;
                    case "|":
                        VMWriter.WriteArithmetic(ArithmeticOp.Or);
                        break;
                    case "<":
                        VMWriter.WriteArithmetic(ArithmeticOp.Lt);
                        break;
                    case ">":
                        VMWriter.WriteArithmetic(ArithmeticOp.Gt);
                        break;
                    case "=":
                        VMWriter.WriteArithmetic(ArithmeticOp.Eq);
                        break;
                    default:
                        break;
                }
                token = tokenQueue.Peek();
            }

            output.Add(PutCloseTag(indent, "expression"));

            return output.ToArray();
        }

        // integerConstant | stringConstant | keywordConstant |
        // varName | varName '[' expression ']' | subroutineCall |
        // '(' expression ')' | unaryOp term
        private string[] CompileTerm(int indent)
        {
            List<string> output = new List<string>();

            output.Add(PutOpenTag(indent, "term"));

            var token = tokenQueue.Peek();

            var queueEnumerator = tokenQueue.GetEnumerator();

            queueEnumerator.MoveNext();
            queueEnumerator.MoveNext();

            var nextToken = queueEnumerator.Current;

            if (Constants.unaryOp.Contains(token.Value[0]))
            {
                string unaryOp = token.Value;
                output.Add(PutToken(indent + 1));
                output.AddRange(CompileTerm(indent + 1));
                switch (unaryOp)
                {
                    case "-":
                        VMWriter.WriteArithmetic(ArithmeticOp.Neg);
                        break;
                    case "~":
                        VMWriter.WriteArithmetic(ArithmeticOp.Not);
                        break;
                    default:
                        break;
                }
            }
            else if (token.Key == TokenType.identifier && nextToken.Value == ".")
            {
                // subroutineName '(' expressionList ')' | (className |
                // varName) '.' subroutineName '(' expressionList ')'

                token = tokenQueue.Dequeue();
                string classname = token.Value;
                output.Add(PutLineTag(indent + 1, Enum.GetName(typeof(TokenType), token.Key), token.Value, "class, used"));
                output.Add(PutToken(indent + 1));
                token = tokenQueue.Dequeue();
                string funcname = token.Value;
                output.Add(PutLineTag(indent + 1, Enum.GetName(typeof(TokenType), token.Key), token.Value, "function, used"));
                output.Add(PutToken(indent + 1));
                int ellen = CompileExpressionList(indent + 1);
                try
                {
                    MetaSymbolTable.KindOf(classname);
                    Segment seg = Segment.Local;
                    switch (MetaSymbolTable.KindOf(classname))
                    {
                        case SymbolKind.Static:
                            seg = Segment.Static;
                            break;
                        case SymbolKind.Field:
                            seg = Segment.This;
                            break;
                        case SymbolKind.Arg:
                            seg = Segment.Argument;
                            break;
                        case SymbolKind.Var:
                            seg = Segment.Local;
                            break;
                        case SymbolKind.None:
                            break;
                        default:
                            break;
                    }
                    VMWriter.WritePush(seg, MetaSymbolTable.IndexOf(classname));
                    ellen += 1;
                }
                catch
                {

                }
                try
                {
                    VMWriter.WriteCall(MetaSymbolTable.TypeOf(classname) + "." + funcname, ellen);
                } catch
                {
                    VMWriter.WriteCall(classname + "." + funcname, ellen);
                }
                output.Add(PutToken(indent + 1));
            }
            else if (token.Key == TokenType.identifier && nextToken.Value == "(")
            {
                token = tokenQueue.Dequeue();
                string funcname = token.Value;
                output.Add(PutLineTag(indent + 1, Enum.GetName(typeof(TokenType), token.Key), token.Value, "function, used"));
                output.Add(PutToken(indent + 1));
                int ellen = CompileExpressionList(indent + 1);
                VMWriter.WritePush(Segment.Pointer, 0);
                VMWriter.WriteCall(ClassName + "." + funcname, ellen + 1);
                output.Add(PutToken(indent + 1));
            }
            else if (nextToken.Value == "[")
            {
                token = tokenQueue.Dequeue();
                string name = token.Value;

                Segment sega = Segment.Local;
                switch (MetaSymbolTable.KindOf(name))
                {
                    case SymbolKind.Static:
                        sega = Segment.Static;
                        break;
                    case SymbolKind.Field:
                        sega = Segment.This;
                        break;
                    case SymbolKind.Arg:
                        sega = Segment.Argument;
                        break;
                    case SymbolKind.Var:
                        sega = Segment.Local;
                        break;
                    case SymbolKind.None:
                        break;
                    default:
                        break;
                }
                VMWriter.WritePush(sega, MetaSymbolTable.IndexOf(name));
                output.Add(PutToken(indent + 1));
                output.AddRange(CompileExpression(indent + 1));
                VMWriter.WriteArithmetic(ArithmeticOp.Add);
                output.Add(PutToken(indent + 1));

                VMWriter.WritePop(Segment.Pointer, 1);
                VMWriter.WritePush(Segment.That, 0);
            }
            else if (token.Value == "(")
            {
                output.Add(PutToken(indent + 1));
                output.AddRange(CompileExpression(indent + 1));
                output.Add(PutToken(indent + 1));
            }
            else if (token.Key == TokenType.identifier)
            {
                token = tokenQueue.Dequeue();
                string name = token.Value;
                output.Add(PutLineTag(indent + 1, Enum.GetName(typeof(TokenType), token.Key), token.Value, "name=" + name + " " + "type=" + MetaSymbolTable.TypeOf(name) + " " + "kind=" + Enum.GetName(typeof(SymbolKind), MetaSymbolTable.KindOf(name)) + " " + "index=" + MetaSymbolTable.IndexOf(name) + " " + "used"));
                Segment seg = Segment.Local;
                switch (MetaSymbolTable.KindOf(name))
                {
                    case SymbolKind.Static:
                        seg = Segment.Static;
                        break;
                    case SymbolKind.Field:
                        seg = Segment.This;
                        break;
                    case SymbolKind.Arg:
                        seg = Segment.Argument;
                        break;
                    case SymbolKind.Var:
                        seg = Segment.Local;
                        break;
                    case SymbolKind.None:
                        break;
                    default:
                        break;
                }
                VMWriter.WritePush(seg, MetaSymbolTable.IndexOf(name));
            }
            else
            {
                switch (token.Key)
                {
                    case TokenType.keyword:
                        switch(token.Value)
                        {
                            case "this":
                                VMWriter.WritePush(Segment.Pointer, 0);
                                break;
                            case "false":
                            case "null":
                                VMWriter.WritePush(Segment.Constant, 0);
                                break;
                            case "true":
                                VMWriter.WritePush(Segment.Constant, 0);
                                VMWriter.WriteArithmetic(ArithmeticOp.Not);
                                break;
                        }
                        break;
                    case TokenType.symbol:
                        break;
                    case TokenType.integerConstant:
                        VMWriter.WritePush(Segment.Constant, int.Parse(token.Value));
                        break;
                    case TokenType.stringConstant:
                        string str = token.Value;
                        VMWriter.WritePush(Segment.Constant, str.Length);
                        VMWriter.WriteCall("String.new", 1);
                        foreach(char c in str)
                        {
                            VMWriter.WritePush(Segment.Constant, c);
                            VMWriter.WriteCall("String.appendChar", 2);
                        }
                        break;
                    default:
                        break;
                }
                output.Add(PutToken(indent + 1));
            }

            output.Add(PutCloseTag(indent, "term"));

            return output.ToArray();
        }

        // (expression (',' expression)* )?
        private int CompileExpressionList(int indent)
        {
            List<string> output = new List<string>();

            int ExpressionListLength = 0;
            output.Add(PutOpenTag(indent, "expressionList"));

            var token = tokenQueue.Peek();
            if (token.Value != ")")
            {
                ExpressionListLength = 1;
                output.AddRange(CompileExpression(indent + 1));
                token = tokenQueue.Peek();
                while (token.Value == ",")
                {
                    ExpressionListLength++;
                    output.Add(PutToken(indent + 1));
                    output.AddRange(CompileExpression(indent + 1));
                    token = tokenQueue.Peek();
                }
            }

            output.Add(PutCloseTag(indent, "expressionList"));

            return ExpressionListLength;
        }

        private string PutToken(int indent)
        {
            var token = tokenQueue.Dequeue();
            return PutLineTag(indent, Enum.GetName(typeof(TokenType), token.Key), token.Value);
        }

        private string PutOpenTag(int indent, string name)
        {
            return new string(' ', indent * Constants.indentLevel) + '<' + name + '>';
        }

        private string PutCloseTag(int indent, string name)
        {
            return new string(' ', indent * Constants.indentLevel) + "</" + name + '>';
        }

        private string PutLineTag(int indent, string name, string value, string add = "")
        {
            return new string(' ', indent * Constants.indentLevel) + '<' + name + " " + add + "> " + SecurityElement.Escape(value) + " </" + name + '>';
        }
    }
}
