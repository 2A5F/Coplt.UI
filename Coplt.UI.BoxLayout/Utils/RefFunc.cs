namespace Coplt.UI.BoxLayout.Utilities;

public delegate R RefFunc<A, in T, in U, out R>(ref A a, T t, U u)
    where A : allows ref struct
    where T : allows ref struct
    where U : allows ref struct
    where R : allows ref struct;
