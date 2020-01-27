namespace ImmutableObjectLib
open System

/////////////////////////////////////////////////////////////////////////
/////      EXAMPLE IMPLEMENTATION Of an imutable Message          ///////
/////      with update function and interface inheritance         ///////
/////////////////////////////////////////////////////////////////////////
type public Priority = VeryLow=100 | Low=200 | Medium=300 | High=400 | VeryHigh=500


// General Simulation Element Interface
type public ISumulationElement = 
    abstract member Key : Guid with get

// Raw Message Definition
type public IRawMessage = 
    abstract Target : string with get
    abstract Object: obj with get
    abstract Priority: Priority with get
    abstract CompareTo : IRawMessage -> int
   // abstract member Col: ReadOnlyCollection<obj>

/// Merge Interfaces to one interface for SimulationMessage 
type ISumulationMessage = 
    inherit ISumulationElement 
    inherit IRawMessage

type public Item =
    {
        Key : Guid
        Text : String
    }
    with member this.Update t = { this with Text = t }

type public ImutableMessage =
    { Key : Guid
      Target : string
      Object: obj
      List: System.Collections.Generic.List<Item>
      Priority: Priority } 
    interface ISumulationMessage with 
        member this.Key  with get() = this.Key
        member this.Target with get() = this.Target
        member this.Object with get() = this.Object
        member this.Priority with get() = this.Priority
        member this.CompareTo(other) = if this.Priority < other.Priority then -1 else 
                                        if this.Priority > other.Priority then 1 else 0
    // Returns new Object with Updated Due
    member this.UpdatePriority p = { this with Priority = p }
    

type public ImutableElementMessage =
    { Key : Guid
      Target : string
      Object: obj
      List: System.Collections.Generic.List<Item>
      Priority: Priority 
      } 
    interface ISumulationElement with 
        member this.Key  with get() = this.Key
    // Returns new Object with Updated Due
    member this.UpdatePriority p = { this with Priority = p }
    

/// Second option do define a Class with Prototype Gernerator for all elements 
/// If you require an entirely new copy of an Object with a new Guid.

type public IWorkItem = 
    abstract member Key : Guid with get
    abstract member WorkItem : string // with get // Note: seems to be not neccecary
type public WorkItem(workItem) = 
    new (prototype: WorkItem, ?workItem) =
        WorkItem(defaultArg workItem prototype.WorkItem ) 
    member val Key = Guid.NewGuid()
    member val WorkItem = workItem
    // Third Way to Clone an object with new Propertyies, sadly doesnt work with c#
    // member this.Clone() = (this :> System.ICloneable).Clone() :?> WorkItem
    interface IWorkItem with 
        member this.Key with get() = this.Key
        member this.WorkItem with get() = this.WorkItem
    // Third Way requierd Extension.
    //// interface ICloneable with 
    ////     member this.Clone() = box (new WorkItem(workItem = this.WorkItem))


