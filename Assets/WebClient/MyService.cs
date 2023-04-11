//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Web.Services;
using System.Web.Services.Protocols;
using System.Xml.Serialization;

// 
// This source code was auto-generated by Web Services Description Language Utility
//Mono Framework v4.0.30319.42000
//


/// <remarks/>
[System.CodeDom.Compiler.GeneratedCodeAttribute("wsdl", "0.0.0.0")]
[System.Diagnostics.DebuggerStepThroughAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Web.Services.WebServiceBindingAttribute(Name="MyServiceSoap", Namespace= "http://192.168.1.32:9000/")]
public partial class MyService : System.Web.Services.Protocols.SoapHttpClientProtocol {
    
    private System.Threading.SendOrPostCallback AddOperationCompleted;
    
    private System.Threading.SendOrPostCallback GetFileNamesOperationCompleted;
    
    private System.Threading.SendOrPostCallback GetFileOperationCompleted;
    
    /// <remarks/>
    public MyService() {
        this.Url = "http://192.168.1.32:9000/MyService.asmx";
    }
    
    /// <remarks/>
    public event AddCompletedEventHandler AddCompleted;
    
    /// <remarks/>
    public event GetFileNamesCompletedEventHandler GetFileNamesCompleted;
    
    /// <remarks/>
    public event GetFileCompletedEventHandler GetFileCompleted;
    
    /// <remarks/>
    [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://192.168.1.32:9000/Add", RequestNamespace= "http://192.168.1.32:9000/", ResponseNamespace= "http://192.168.1.32:9000/", Use=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
    public int Add(int a, int b) {
        object[] results = this.Invoke("Add", new object[] {
                    a,
                    b});
        return ((int)(results[0]));
    }
    
    /// <remarks/>
    public void AddAsync(int a, int b) {
        this.AddAsync(a, b, null);
    }
    
    /// <remarks/>
    public void AddAsync(int a, int b, object userState) {
        if ((this.AddOperationCompleted == null)) {
            this.AddOperationCompleted = new System.Threading.SendOrPostCallback(this.OnAddOperationCompleted);
        }
        this.InvokeAsync("Add", new object[] {
                    a,
                    b}, this.AddOperationCompleted, userState);
    }
    
    private void OnAddOperationCompleted(object arg) {
        if ((this.AddCompleted != null)) {
            System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
            this.AddCompleted(this, new AddCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
        }
    }
    
    /// <remarks/>
    [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://192.168.1.32:9000/GetFileNames", RequestNamespace= "http://192.168.1.32:9000/", ResponseNamespace= "http://192.168.1.32:9000/", Use=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
    public string[] GetFileNames() {
        object[] results = this.Invoke("GetFileNames", new object[0]);
        return ((string[])(results[0]));
    }
    
    /// <remarks/>
    public void GetFileNamesAsync() {
        this.GetFileNamesAsync(null);
    }
    
    /// <remarks/>
    public void GetFileNamesAsync(object userState) {
        if ((this.GetFileNamesOperationCompleted == null)) {
            this.GetFileNamesOperationCompleted = new System.Threading.SendOrPostCallback(this.OnGetFileNamesOperationCompleted);
        }
        this.InvokeAsync("GetFileNames", new object[0], this.GetFileNamesOperationCompleted, userState);
    }
    
    private void OnGetFileNamesOperationCompleted(object arg) {
        if ((this.GetFileNamesCompleted != null)) {
            System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
            this.GetFileNamesCompleted(this, new GetFileNamesCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
        }
    }
    
    /// <remarks/>
    [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://192.168.1.32:9000/GetFile", RequestNamespace= "http://192.168.1.32:9000/", ResponseNamespace= "http://192.168.1.32:9000/", Use=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
    public string[] GetFile(string path) {
        object[] results = this.Invoke("GetFile", new object[] {
                    path});
        return ((string[])(results[0]));
    }
    
    /// <remarks/>
    public void GetFileAsync(string path) {
        this.GetFileAsync(path, null);
    }
    
    /// <remarks/>
    public void GetFileAsync(string path, object userState) {
        if ((this.GetFileOperationCompleted == null)) {
            this.GetFileOperationCompleted = new System.Threading.SendOrPostCallback(this.OnGetFileOperationCompleted);
        }
        this.InvokeAsync("GetFile", new object[] {
                    path}, this.GetFileOperationCompleted, userState);
    }
    
    private void OnGetFileOperationCompleted(object arg) {
        if ((this.GetFileCompleted != null)) {
            System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
            this.GetFileCompleted(this, new GetFileCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
        }
    }
    
    /// <remarks/>
    public new void CancelAsync(object userState) {
        base.CancelAsync(userState);
    }
}

/// <remarks/>
[System.CodeDom.Compiler.GeneratedCodeAttribute("wsdl", "0.0.0.0")]
public delegate void AddCompletedEventHandler(object sender, AddCompletedEventArgs e);

/// <remarks/>
[System.CodeDom.Compiler.GeneratedCodeAttribute("wsdl", "0.0.0.0")]
[System.Diagnostics.DebuggerStepThroughAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
public partial class AddCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs {
    
    private object[] results;
    
    internal AddCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) : 
            base(exception, cancelled, userState) {
        this.results = results;
    }
    
    /// <remarks/>
    public int Result {
        get {
            this.RaiseExceptionIfNecessary();
            return ((int)(this.results[0]));
        }
    }
}

/// <remarks/>
[System.CodeDom.Compiler.GeneratedCodeAttribute("wsdl", "0.0.0.0")]
public delegate void GetFileNamesCompletedEventHandler(object sender, GetFileNamesCompletedEventArgs e);

/// <remarks/>
[System.CodeDom.Compiler.GeneratedCodeAttribute("wsdl", "0.0.0.0")]
[System.Diagnostics.DebuggerStepThroughAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
public partial class GetFileNamesCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs {
    
    private object[] results;
    
    internal GetFileNamesCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) : 
            base(exception, cancelled, userState) {
        this.results = results;
    }
    
    /// <remarks/>
    public string[] Result {
        get {
            this.RaiseExceptionIfNecessary();
            return ((string[])(this.results[0]));
        }
    }
}

/// <remarks/>
[System.CodeDom.Compiler.GeneratedCodeAttribute("wsdl", "0.0.0.0")]
public delegate void GetFileCompletedEventHandler(object sender, GetFileCompletedEventArgs e);

/// <remarks/>
[System.CodeDom.Compiler.GeneratedCodeAttribute("wsdl", "0.0.0.0")]
[System.Diagnostics.DebuggerStepThroughAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
public partial class GetFileCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs {
    
    private object[] results;
    
    internal GetFileCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) : 
            base(exception, cancelled, userState) {
        this.results = results;
    }
    
    /// <remarks/>
    public string[] Result {
        get {
            this.RaiseExceptionIfNecessary();
            return ((string[])(this.results[0]));
        }
    }
}
