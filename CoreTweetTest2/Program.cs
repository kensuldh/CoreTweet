﻿#if TEST
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
#endif

public class ApiEndpoint
{
    public string Name { get; set; }

    public string Request { get; set; }

    public string ReturnType { get; set; }

    public ApiType Type { get; set; }

    public string ReservedName { get; set; }

    public string JsonPath { get; set; }

    public string Uri { get; set; }

    public string[] Description { get; set; }

    public string Returns { get; set; }

    public Tuple<string, string, string>[] Params { get; set; }

    public string MethodDefinition
    {
        get
        {
            switch (this.Type)
            {
                case ApiType.Normal:
                    return string.Format("public {0} {1}", this.ReturnType, this.Name);
                case ApiType.IE:
                    return string.Format("public IEnumerable<{0}> {1}", this.ReturnType, this.Name);
                case ApiType.Listed:
                    return string.Format("public ListedResponse<{0}> {1}", this.ReturnType, this.Name);
                case ApiType.Cursored:
                    return string.Format("public Cursored<{0}> {1}", this.ReturnType, this.Name);
                default: 
                    throw new ArgumentException("");
            }
        }
    }

    public string MethodDefinitionAsync
    {
        get
        {
            switch (this.Type)
            {
                case ApiType.Normal:
                    return string.Format("public Task<{0}> {1}", this.ReturnType, this.Name);
                case ApiType.Listed:
                    return string.Format("public Task<ListedResponse<{0}>> {1}", this.ReturnType, this.Name);
                case ApiType.Cursored:
                    return string.Format("public Task<Cursored<{0}>> {1}", this.ReturnType, this.Name);
                default: 
                    throw new ArgumentException("Async IE<> endpoints are illegal");
            }
        }
    }

    public string JsonPathOrEmpty { get { return JsonPath != null ? ", " + JsonPath : ""; } }

    public Tuple<string, string> PE
    {
        get
        {
            var s1 = this.MethodDefinition + "(params Expression<Func<string, object>>[] parameters)";
            var s2 = "";
            if (this.ReservedName == null)
                switch (this.Type)
                {
                    case ApiType.Normal:
                    s2 = string.Format("return this.Tokens.AccessApi<{0}>(MethodType.{1}, \"{2}\", parameters{3});", this.ReturnType, this.Request, this.Uri, JsonPathOrEmpty);
                        break;
                    case ApiType.IE:
                    case ApiType.Listed:
                    s2 = string.Format("return this.Tokens.AccessApiArray<{0}>(MethodType.{1}, \"{2}\", parameters{3});", this.ReturnType, this.Request, this.Uri, JsonPathOrEmpty);
                        break;
                    case ApiType.Cursored:
                    s2 = string.Format("return this.Tokens.AccessApi<Cursored<{0}>>(MethodType.{1}, \"{2}\", parameters{3});", this.ReturnType, this.Request, this.Uri, JsonPathOrEmpty);
                        break;
                }
            else
            {
                switch (this.Type)
                {
                    case ApiType.Normal:
                        s2 = string.Format(
                            "return this.Tokens.AccessParameterReservedApi<{0}>(MethodType.{1}, \"{2}\", \"{3}\", InternalUtils.ExpressionsToDictionary(parameters));"
                                     , this.ReturnType, this.Request, this.Uri, this.ReservedName);
                        break;
                    case ApiType.IE:
                    case ApiType.Listed:
                        s2 = string.Format(
                            "return this.Tokens.AccessParameterReservedApiArray<{0}>(MethodType.{1}, \"{2}\", \"{3}\", InternalUtils.ExpressionsToDictionary(parameters));"
                                     , this.ReturnType, this.Request, this.Uri, this.ReservedName);
                        break;
                }
            }
            return Tuple.Create(s1, s2);
        }
    }

    public Tuple<string, string> ID
    {
        get
        {
            var s1 = this.MethodDefinition + "(IDictionary<string, object> parameters)";
            var s2 = "";
            if (this.ReservedName == null)
                switch (this.Type)
                {
                    case ApiType.Normal:
                    s2 = string.Format("return this.Tokens.AccessApi<{0}>(MethodType.{1}, \"{2}\", parameters{3});", this.ReturnType, this.Request, this.Uri, JsonPathOrEmpty);
                        break;
                    case ApiType.IE:
                    case ApiType.Listed:
                    s2 = string.Format("return this.Tokens.AccessApiArray<{0}>(MethodType.{1}, \"{2}\", parameters{3});", this.ReturnType, this.Request, this.Uri, JsonPathOrEmpty);
                        break;
                    case ApiType.Cursored:
                    s2 = string.Format("return this.Tokens.AccessApi<Cursored<{0}>>(MethodType.{1}, \"{2}\", parameters{3});", this.ReturnType, this.Request, this.Uri, JsonPathOrEmpty);
                        break;
                }
            else
            {
                switch (this.Type)
                {
                    case ApiType.Normal:
                        s2 = string.Format(
                            "return this.Tokens.AccessParameterReservedApi<{0}>(MethodType.{1}, \"{2}\", \"{3}\", parameters);"
                            , this.ReturnType, this.Request, this.Uri, this.ReservedName);
                        break;
                    case ApiType.IE:
                    case ApiType.Listed:
                        s2 = string.Format(
                            "return this.Tokens.AccessParameterReservedApiArray<{0}>(MethodType.{1}, \"{2}\", \"{3}\", parameters);"
                            , this.ReturnType, this.Request, this.Uri, this.ReservedName);
                        break;
                }
            }
            return Tuple.Create(s1, s2);
        }
    }

    public Tuple<string, string> T
    {
        get
        {
            var s1 = this.MethodDefinition + "<T>(T parameters)";
            var s2 = "";
            if (this.ReservedName == null)
                switch (this.Type)
            {
                case ApiType.Normal:
                    s2 = string.Format("return this.Tokens.AccessApi<{0}, T>(MethodType.{1}, \"{2}\", parameters{3});", this.ReturnType, this.Request, this.Uri, JsonPathOrEmpty);
                    break;
                case ApiType.IE:
                case ApiType.Listed:
                    s2 = string.Format("return this.Tokens.AccessApiArray<{0}, T>(MethodType.{1}, \"{2}\", parameters{3});", this.ReturnType, this.Request, this.Uri, JsonPathOrEmpty);
                    break;
                case ApiType.Cursored:
                    s2 = string.Format("return this.Tokens.AccessApi<Cursored<{0}>, T>(MethodType.{1}, \"{2}\", parameters{3});", this.ReturnType, this.Request, this.Uri, JsonPathOrEmpty);
                    break;
            }
            else
            {
                switch (this.Type)
                {
                    case ApiType.Normal:
                        s2 = string.Format(
                            "return this.Tokens.AccessParameterReservedApi<{0}>(MethodType.{1}, \"{2}\", \"{3}\", InternalUtils.ResolveObject(parameters));"
                            , this.ReturnType, this.Request, this.Uri, this.ReservedName);
                        break;
                    case ApiType.IE:
                    case ApiType.Listed:
                        s2 = string.Format(
                            "return this.Tokens.AccessParameterReservedApiArray<{0}>(MethodType.{1}, \"{2}\", \"{3}\", InternalUtils.ResolveObject(parameters));"
                            , this.ReturnType, this.Request, this.Uri, this.ReservedName);
                        break;
                }
            }
            return Tuple.Create(s1, s2);
        }
    }

    public Tuple<string, string> PEAsync
    {
        get
        {
            var s1 = this.MethodDefinitionAsync + "(params Expression<Func<string, object>>[] parameters)";
            var s2 = "";
            if (this.ReservedName == null)
                switch (this.Type)
            {
                case ApiType.Normal:
                    s2 = string.Format("return this.Tokens.AccessApiAsync<{0}>(MethodType.{1}, \"{2}\", parameters{3});", this.ReturnType, this.Request, this.Uri, JsonPathOrEmpty);
                    break;
                case ApiType.IE:
                case ApiType.Listed:
                    s2 = string.Format("return this.Tokens.AccessApiArrayAsync<{0}>(MethodType.{1}, \"{2}\", parameters{3});", this.ReturnType, this.Request, this.Uri, JsonPathOrEmpty);
                    break;
                case ApiType.Cursored:
                    s2 = string.Format("return this.Tokens.AccessApiAsync<Cursored<{0}>>(MethodType.{1}, \"{2}\", parameters{3});", this.ReturnType, this.Request, this.Uri, JsonPathOrEmpty);
                    break;
            }
            else
            {
                switch (this.Type)
                {
                    case ApiType.Normal:
                        s2 = string.Format(
                            "return this.Tokens.AccessParameterReservedApiAsync<{0}>(MethodType.{1}, \"{2}\", \"{3}\", InternalUtils.ExpressionsToDictionary(parameters), CancellationToken.None);"
                            , this.ReturnType, this.Request, this.Uri, this.ReservedName);
                        break;
                    case ApiType.IE:
                    case ApiType.Listed:
                        s2 = string.Format(
                            "return this.Tokens.AccessParameterReservedApiArrayAsync<{0}>(MethodType.{1}, \"{2}\", \"{3}\", InternalUtils.ExpressionsToDictionary(parameters), CancellationToken.None);"
                            , this.ReturnType, this.Request, this.Uri, this.ReservedName);
                        break;
                }
            }
            return Tuple.Create(s1, s2);
        }
    }

    public Tuple<string, string> IDAsync
    {
        get
        {
            var s1 = this.MethodDefinitionAsync + "(IDictionary<string, object> parameters, CancellationToken cancellationToken = default(CancellationToken))";
            var s2 = "";
            if (this.ReservedName == null)
                switch (this.Type)
            {
                case ApiType.Normal:
                    s2 = string.Format("return this.Tokens.AccessApiAsync<{0}>(MethodType.{1}, \"{2}\", parameters, cancellationToken{3});", this.ReturnType, this.Request, this.Uri, JsonPathOrEmpty);
                    break;
                case ApiType.IE:
                case ApiType.Listed:
                    s2 = string.Format("return this.Tokens.AccessApiArrayAsync<{0}>(MethodType.{1}, \"{2}\", parameters, cancellationToken{3});", this.ReturnType, this.Request, this.Uri, JsonPathOrEmpty);
                    break;
                case ApiType.Cursored:
                    s2 = string.Format("return this.Tokens.AccessApiAsync<Cursored<{0}>>(MethodType.{1}, \"{2}\", parameters, cancellationToken{3});", this.ReturnType, this.Request, this.Uri, JsonPathOrEmpty);
                    break;
            }
            else
            {
                switch (this.Type)
                {
                    case ApiType.Normal:
                        s2 = string.Format(
                            "return this.Tokens.AccessParameterReservedApiAsync<{0}>(MethodType.{1}, \"{2}\", \"{3}\", parameters, cancellationToken);"
                            , this.ReturnType, this.Request, this.Uri, this.ReservedName);
                        break;
                    case ApiType.IE:
                    case ApiType.Listed:
                        s2 = string.Format(
                            "return this.Tokens.AccessParameterReservedApiArrayAsync<{0}>(MethodType.{1}, \"{2}\", \"{3}\", parameters, cancellationToken);"
                            , this.ReturnType, this.Request, this.Uri, this.ReservedName);
                        break;
                }
            }
            return Tuple.Create(s1, s2);
        }
    }

    public Tuple<string, string> TAsync
    {
        get
        {
            var s1 = this.MethodDefinitionAsync + "<T>(T parameters, CancellationToken cancellationToken = default(CancellationToken))";
            var s2 = "";
            if (this.ReservedName == null)
                switch (this.Type)
            {
                case ApiType.Normal:
                    s2 = string.Format("return this.Tokens.AccessApiAsync<{0}, T>(MethodType.{1}, \"{2}\", parameters, cancellationToken{3});", this.ReturnType, this.Request, this.Uri, JsonPathOrEmpty);
                    break;
                case ApiType.IE:
                case ApiType.Listed:
                    s2 = string.Format("return this.Tokens.AccessApiArrayAsync<{0}, T>(MethodType.{1}, \"{2}\", parameters, cancellationToken{3});", this.ReturnType, this.Request, this.Uri, JsonPathOrEmpty);
                    break;
                case ApiType.Cursored:
                    s2 = string.Format("return this.Tokens.AccessApiAsync<Cursored<{0}>, T>(MethodType.{1}, \"{2}\", parameters, cancellationToken{3});", this.ReturnType, this.Request, this.Uri, JsonPathOrEmpty);
                    break;
            }
            else
            {
                switch (this.Type)
                {
                    case ApiType.Normal:
                        s2 = string.Format(
                            "return this.Tokens.AccessParameterReservedApiAsync<{0}>(MethodType.{1}, \"{2}\", \"{3}\", InternalUtils.ResolveObject(parameters), cancellationToken);"
                            , this.ReturnType, this.Request, this.Uri, this.ReservedName);
                        break;
                    case ApiType.IE:
                    case ApiType.Listed:
                        s2 = string.Format(
                            "return this.Tokens.AccessParameterReservedApiArrayAsync<{0}>(MethodType.{1}, \"{2}\", \"{3}\", InternalUtils.ResolveObject(parameters), cancellationToken);"
                            , this.ReturnType, this.Request, this.Uri, this.ReservedName);
                        break;
                }
            }
            return Tuple.Create(s1, s2);
        }
    }

    public List<Tuple<string, string>> Methods
    {
        get
        {
            var l = new List<Tuple<string, string>>();
            l.AddRange(new []{ this.PE, this.ID, this.T });
            if (this.Type == ApiType.Cursored)
            {
                var name = "Cursored<" + this.ReturnType + "> Enumerate" +  this.Name;
                foreach (var x in new []
                    {
                        name + "(EnumerateMode mode, params Expression<Func<string, object>>[] parameters)",
                        name + "(EnumerateMode mode, params IDictionary<string, object> parameters)",
                        name + "<T>(EnumerateMode mode, T parameters)"
                    })
                    l.Add(Tuple.Create(x, string.Format("return Cursored<{0}>.Enumerate(this.Tokens, \"{1}\", mode, parameters{2});", this.ReturnType, this.Uri, JsonPathOrEmpty)));
            }
            return l;
        }
    }


    public List<Tuple<string, string>> MethodsAsync
    {
        get
        {
            return new List<Tuple<string, string>>(new []{ this.PEAsync, this.IDAsync, this.TAsync });
        }
    }
}
    
public class RawLines : ApiEndpoint
{
    public string[] Lines { get; set; }
}

public enum ApiType
{
    Normal,
    IE,
    Listed,
    Cursored
}

public enum Mode
{
    none,
    endpoint,
    description,
    returns,
    prms,
    with
}

public class Indent
{
    int indent { get; set; }

    public int Spaces { get; set; }

    public Indent(int i, int s = 4)
    {
        this.indent = i;
        this.Spaces = s;
    }

    public void Inc()
    {
        indent = indent + 1;
    }

    public void Dec()
    {
        indent = indent - 1;
    }

    public override string ToString()
    {
        return string.Concat(Enumerable.Range(1, Spaces * indent).Select(_ => " "));
    }
}

public class ApiParent
{
    public string Name { get; set; }

    public string Description { get; set; }

    public ApiEndpoint[] Endpoints { get; set; }

    public static ApiParent Parse(string text)
    {
        var ret = new ApiParent();

        var lines = text.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
        foreach (var i in lines)
            Console.WriteLine(i);
        ret.Name = lines.First(x => x.StartsWith("#namespace")).Split(' ')[1];
        ret.Description = lines.First(x => x.StartsWith("#description")).Replace("#description ", "");

        var es = new List<ApiEndpoint>();

        var mode = Mode.none;
        var now = new ApiEndpoint();
        var s = new List<string>();
        var s2 = new List<string>();
        var commenting = false;

        foreach (var i in lines)
        {
            var l = string.Concat(i.SkipWhile(x => x == '\t' || x == ' '));
            if (l.StartsWith("/*") || l.StartsWith("#raw"))
            {
                commenting = true;
            }
            else if (l.StartsWith("#endraw") || l.StartsWith("*/"))
            {
                commenting = false;
                if (l.StartsWith("#endraw"))
                {
                    es.Add(new RawLines(){ Lines = s2.ToArray() });
                }
                s2.Clear();
            }
            else if (commenting)
            {
                s2.Add(i); 
            }
            else if (l.StartsWith("endpoint"))
            {
                var x = l.Split(' ');
                now.Name = x[2];
                var rt = x[1];
                if (rt.StartsWith("IE"))
                {
                    now.ReturnType = rt.Split(new []{ '<', '>' })[1];
                    now.Type = ApiType.IE;
                }
                else if (rt.StartsWith("Listed"))
                {
                    now.ReturnType = rt.Split(new []{ '<', '>' })[1];
                    now.Type = ApiType.Listed;
                }
                else if (rt.StartsWith("Cursored"))
                {
                    now.ReturnType = rt.Split(new []{ '<', '>' })[1];
                    now.Type = ApiType.Cursored;
                }
                else
                {
                    now.ReturnType = x[1];
                    now.Type = ApiType.Normal;
                }
                now.Request = x[4];
                now.Uri = x[5];
                if (now.Uri.Contains("{"))
                {
                    now.ReservedName = now.Uri.Split(new []{ '{', '}' })[1];
                }
                mode = Mode.endpoint;
            }
            else if (l.StartsWith("description"))
            {
                mode = Mode.description;
            }
            else if (l.StartsWith("returns"))
            {
                mode = Mode.returns;
            }
            else if (l.StartsWith("params"))
            {
                mode = Mode.prms;
            }
            else if (l.StartsWith("with"))
            {
                mode = Mode.with;
            }
            else if (l.StartsWith("{"))
            {
            }
            else if (l.StartsWith("}"))
                switch (mode)
                {
                    case Mode.none:
                        break;
                    case Mode.description:
                        now.Description = s.ToArray();
                        s.Clear();
                        mode = Mode.endpoint;
                        break;
                    case Mode.returns:
                        now.Returns = string.Join(Environment.NewLine, s);
                        s.Clear();
                        mode = Mode.endpoint;
                        break;
                    case Mode.prms:
                        now.Params = s.Select(x =>
                            {
                                var y = x.Split(' ');
                                return Tuple.Create(y[0], y[1], y[2]);
                            }).ToArray();
                        s.Clear();
                        mode = Mode.endpoint;
                        break;
                    case Mode.with:
                        foreach (var x in s)
                        {
                            if (x.StartsWith("JsonPath="))
                            {
                                now.JsonPath = x.Replace("JsonPath=", "");
                            }
                            s.Clear();
                        }
                        break;
                    case Mode.endpoint:
                        es.Add(now);
                        now = new ApiEndpoint();
                        mode = Mode.none;
                        s.Clear();
                        break;
                }
            else if (!l.StartsWith("#") && !l.StartsWith("//") && !l.All(x => char.IsWhiteSpace(x)) && l != "")
                s.Add(l);
        }

        ret.Endpoints = es.ToArray();

        return ret;
    }
}

#if TEST
public class Test
{
    static void Main(string[] args)
    {
        var x = ApiParent.Parse(File.ReadAllText(args[0]));
        foreach(var y in x.Endpoints)
            Console.WriteLine(y.Methods.Count);
        Console.WriteLine(x);
    }
}
#endif