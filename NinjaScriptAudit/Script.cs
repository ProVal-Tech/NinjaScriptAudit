using System;
using System.Collections.Generic;
using System.Text;

namespace NinjaScriptAudit {

    public class Script {
        public string? Uid { get; set; }
        public string? Name { get; set; }
        public bool IsActive { get; set; }
        public string? Language { get; set; }
        public string? Code { get; set; }
        public int ContentId { get; set; }
        public string? Description { get; set; }
        public string[]? Architecture { get; set; }
        public int[]? CategoriesIds { get; set; }
        public object[]? ScriptParameters { get; set; }
        public string[]? OperatingSystems { get; set; }
        public ScriptVariable[]? ScriptVariables { get; set; }
        public string? DefaultRunAs { get; set; }
        public bool UseFirstParametersOptionAsDefault { get; set; }
        public string? CreatedBy { get; set; }
        public string? LastUpdatedBy { get; set; }
        public float CreatedOn { get; set; }
        public float UpdatedOn { get; set; }
        public int Id { get; set; }
    }
}
