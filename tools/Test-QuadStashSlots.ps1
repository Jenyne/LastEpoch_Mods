$ErrorActionPreference = 'Stop'
# Exercise the actual occupancy hook bodies with small managed stand-ins. This
# does not instantiate IL2CPP objects or claim to test native stash persistence.
$source = Get-Content (Join-Path $PSScriptRoot '../LastEpoch_Hud/Scripts/Mods/Bank/Bank_Quad.cs') -Raw
function Block([string]$marker) {
    $start = $source.IndexOf($marker)
    if ($start -lt 0) { throw "Missing source block: $marker" }
    $open = $source.IndexOf('{', $start)
    $depth = 1
    $end = $open + 1
    while ($depth -gt 0 -and $end -lt $source.Length) {
        if ($source[$end] -eq '{') { $depth++ }
        if ($source[$end] -eq '}') { $depth-- }
        $end++
    }
    return $source.Substring($start, $end - $start)
}
$helpers = (Block 'private static bool IsQuadContainer') + (Block 'private static bool Fits') + (Block 'private static bool[] GetOccupiedSlots')
$positions = Block 'public static System.Collections.Generic.List<int> SlotsPosition'
$check = (Block 'public class ItemContainer_CheckSlotsOccupied').Replace('[HarmonyPrefix]', '').Replace('static bool Prefix', 'public static bool Prefix')
$set = (Block 'public class ItemContainer_SetSlotsOccupied').Replace('[HarmonyPrefix]', '').Replace('static bool Prefix', 'public static bool Prefix')
$code = @'
using System;
using System.Collections.Generic;
public struct Vector2Int {
 public int x,y; public Vector2Int(int x,int y) {this.x=x;this.y=y;}
 public static bool operator ==(Vector2Int a,Vector2Int b)=>a.x==b.x&&a.y==b.y;
 public static bool operator !=(Vector2Int a,Vector2Int b)=>!(a==b);
 public override bool Equals(object b)=>b is Vector2Int v&&this==v;
 public override int GetHashCode()=>x*397^y;
}
public enum ContainerID { STASH, INVENTORY }
public class Entry {public Vector2Int Position,size;}
public class ItemContainer { public ContainerID id=ContainerID.STASH; public Vector2Int size;
 public IntPtr Pointer; public List<Entry> content=new List<Entry>(); }
public static class NullHelpers {public static bool IsNullOrDestroyed(this object value)=>value==null;}
public static class QuadSlotTests {
 static Vector2Int quad_size=new Vector2Int(24,34);
 static Dictionary<IntPtr,bool[]> occupied_slots=new Dictionary<IntPtr,bool[]>();
'@ + $helpers + 'public class Get {' + $positions + '}' + $check + $set + @'
 static void Assert(bool pass,string name) {if(!pass)throw new Exception(name); Console.WriteLine("PASS: "+name);}
 public static void Run() {
  var normal=new ItemContainer {Pointer=new IntPtr(1),size=new Vector2Int(12,17)};
  var quad=new ItemContainer {Pointer=new IntPtr(2),size=quad_size};
  quad.content.Add(new Entry {Position=new Vector2Int(0,0),size=new Vector2Int(2,2)});
  bool result=true;
  Assert(ItemContainer_CheckSlotsOccupied.Prefix(normal,ref result,new Vector2Int(0,0),new Vector2Int(1,1))&&result,"normal tab uses native slot checks");
  Assert(ItemContainer_SetSlotsOccupied.Prefix(normal,ref result,new Vector2Int(0,0),new Vector2Int(1,1),true),"normal tab uses native slot updates");
  Assert(!ItemContainer_CheckSlotsOccupied.Prefix(quad,ref result,new Vector2Int(1,1),new Vector2Int(1,1))&&result,"missing cache reconstructs existing items");
  Assert(!ItemContainer_CheckSlotsOccupied.Prefix(quad,ref result,new Vector2Int(23,33),new Vector2Int(1,1))&&!result,"last quad slot is accessible");
  Assert(!ItemContainer_SetSlotsOccupied.Prefix(quad,ref result,new Vector2Int(23,33),new Vector2Int(1,1),true)&&result,"successful slot update returns true");
  ItemContainer_CheckSlotsOccupied.Prefix(quad,ref result,new Vector2Int(23,33),new Vector2Int(1,1)); Assert(result,"reserved slot is occupied");
  ItemContainer_SetSlotsOccupied.Prefix(quad,ref result,new Vector2Int(23,33),new Vector2Int(1,1),false);
  ItemContainer_CheckSlotsOccupied.Prefix(quad,ref result,new Vector2Int(23,33),new Vector2Int(1,1)); Assert(!result,"removed item releases slot");
  foreach(var p in new[]{new Vector2Int(-1,0),new Vector2Int(24,0),new Vector2Int(0,34),new Vector2Int(int.MaxValue,0)}) {
   ItemContainer_CheckSlotsOccupied.Prefix(quad,ref result,p,new Vector2Int(1,1)); Assert(result,"invalid position is blocked "+p.x+","+p.y);
   ItemContainer_SetSlotsOccupied.Prefix(quad,ref result,p,new Vector2Int(1,1),true); Assert(!result,"invalid update fails without indexing");
  }
  ItemContainer_CheckSlotsOccupied.Prefix(quad,ref result,new Vector2Int(23,33),new Vector2Int(2,2)); Assert(result,"items cannot cross grid edge");
  var other=new ItemContainer {Pointer=new IntPtr(3),size=quad_size};
  ItemContainer_CheckSlotsOccupied.Prefix(other,ref result,new Vector2Int(0,0),new Vector2Int(1,1)); Assert(!result,"tab occupancy is isolated by container identity");
  occupied_slots.Remove(quad.Pointer);
  ItemContainer_CheckSlotsOccupied.Prefix(quad,ref result,new Vector2Int(0,0),new Vector2Int(1,1)); Assert(result,"cache rebuild preserves occupancy after reset");
 }
}
'@
Add-Type -TypeDefinition $code
[QuadSlotTests]::Run()
