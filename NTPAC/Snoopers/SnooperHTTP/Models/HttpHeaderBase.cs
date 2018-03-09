using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using NTPAC.ApplicationProtocolExport.Core.PduProviders;

namespace SnooperHTTP.Models
{
  public abstract class HttpHeaderBase
  {
    public readonly Dictionary<String, List<String>> Fields = new Dictionary<String, List<String>>();
     
    protected void ParseHeaderValues(PduStreamReader reader)
    {
      String line;
      while ((line = reader.ReadLine()) != null && line != "")
      {
        var fieldNameIndex = line.IndexOf(':');
        var fieldName      = line.Substring(0, fieldNameIndex).ToLower();
        var fieldValue     = line.Substring(fieldNameIndex + 1, line.Length - fieldName.Length - 1).Trim().ToLower();
        if (this.Fields.ContainsKey(fieldName))
        {
          this.Fields[fieldName].Add(fieldValue);
        }
        else
        {
          this.Fields[fieldName] = new List<String> { fieldValue };
        }
      }
    }

    public String FormatFields() => String.Join(Environment.NewLine, this.Fields.Keys.Select(i => $"{i}: {String.Join("; ", this.Fields[i])}"));

    public String GetLastHeaderFieldValue(String fieldName, String defaultFieldValue = null) =>
      this.Fields.TryGetValue(fieldName.ToLower(), out var fieldValues) ? fieldValues.Last() : defaultFieldValue;

    public Boolean HeaderFieldValueEquals(String fieldName, String pattern)
    {
      var headerFieldValue = this.GetLastHeaderFieldValue(fieldName);
      return headerFieldValue != null && headerFieldValue.Equals(pattern.ToLower());
    }

    public override String ToString() => $"{this.StatusLine}{Environment.NewLine}{this.FormatFields()}";

    public abstract String StatusLine { get; }
    
    public abstract String HttpVersion { get; } 
  }
}
