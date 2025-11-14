namespace NinjaScriptAudit {
    public class ScriptVariable {
        public string? Id { get; set; }
        public string? Name { get; set; }
        public string? CalculatedName { get; set; }
        public string? Description { get; set; }
        public string? Type { get; set; }
        public string? Source { get; set; }
        public string? DefaultValue { get; set; }
        public bool Required { get; set; }
        public string[]? ValueList { get; set; }
    }

}
