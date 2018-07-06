namespace ImmutableObjectLib
open System

/////////////////////////////////////////////////////////////////////////
/////      EXAMPLE IMPLEMENTATION Of an imutable Message          ///////
/////      with update function and interface inheritance         ///////
/////////////////////////////////////////////////////////////////////////
// General Simulation Element Interface
type public ISumulationElement = 
    abstract member Key : Guid with get

// Raw Message Definition
type public IRawMessage = 
    abstract Target : string with get
    abstract Source: string with get
    abstract Object: obj with get
    abstract Due: int64 with get
    abstract Priority: int with get
    abstract CompareTo : IRawMessage -> int
   // abstract member Col: ReadOnlyCollection<obj>

/// Merge Interfaces to one interface for SimulationMessage 
type ISumulationMessage = 
    inherit ISumulationElement 
    inherit IRawMessage

type public Message =
    { Key : Guid
      Target : string
      Source: string
      Object: obj
      Due: int64 
      Priority: int} 
    interface ISumulationMessage with 
        member this.Key with get() = this.Key
        member this.Target with get() = this.Target
        member this.Source with get() = this.Source
        member this.Object with get() = this.Object
        member this.Due with get() = this.Due
        member this.Priority with get() = this.Priority
        member this.CompareTo(other) = if this.Priority < other.Priority then -1 else 
                                        if this.Priority > other.Priority then 1 else 0
    // Returns new Object with Updated Due
    member this.UpdateDue d = { this with Due = d }
    
        

