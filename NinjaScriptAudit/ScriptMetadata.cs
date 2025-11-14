using System;
using System.Collections.Generic;
using System.Text;

namespace NinjaScriptAudit {
    public class ScriptMetadata {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public bool Active { get; set; }
        public string? Language { get; set; }
        public string[]? Architecture { get; set; }
        public string[]? OperatingSystems { get; set; }
        public object[]? ScriptParameters { get; set; }
        public ScriptVariable[]? ScriptVariables { get; set; }
        public string? CreatedBy { get; set; }
        public string? LastUpdatedBy { get; set; }
        public float CreatedOn { get; set; }
        public float UpdatedOn { get; set; }
    }

}
