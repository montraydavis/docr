# Namespace: {namespace}

## Class: {class}

### Usings

{{ if class.Usings.Count > 0 }}
{{ each using in class.Usings }}
- {{ using }}
{{ end }}
{{ else }}
- None
{{ end }}

### Properties

{{ each property in class.Properties }}
- **Name**: {{ property.Name }}
  - **Type**: {{ property.Type }}
  - **Access Modifiers**:
    - Public: {{ property.IsPublic }}
    - Static: {{ property.IsStatic }}
    - ReadOnly: {{ property.IsReadOnly }}
    - Virtual: {{ property.IsVirtual }}
    - Override: {{ property.IsOverride }}
    - Sealed: {{ property.IsSealed }}
    - Abstract: {{ property.IsAbstract }}
    - Extern: {{ property.IsExtern }}
    - Unsafe: {{ property.IsUnsafe }}
    - Partial: {{ property.IsPartial }}
    - Const: {{ property.IsConst }}
    - Volatile: {{ property.IsVolatile }}
    - New: {{ property.IsNew }}
    - Internal: {{ property.IsInternal }}
    - Protected: {{ property.IsProtected }}
    - Private: {{ property.IsPrivate }}
    - Attributes: {{ property.Attributes | default: "None" }}
{{ end }}

### Methods

{{ each method in class.Methods }}
- **Method**: {{ method.Name }}
  - **Return Type**: {{ method.ReturnType }}
  - **Parameters**:
    {{ each parameter in method.Parameters }}
    - **Name**: {{ parameter.Name }}
      - **Type**: {{ parameter.Type }}
      - **Attributes**: {{ parameter.Attributes | default: "None" }}
    {{ end }}
  - **Documentation**: {{ method.Documentation.Comment | default: "None" }}
  - **Attributes**: {{ method.Attributes | default: "None" }}
{{ end }}