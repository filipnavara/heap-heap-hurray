using System.Text.Json;
using Graphs;

class Program
{
    public static void Main(string[] args)
    {
        if (args.Length < 2)
        {
            Console.WriteLine("Usage: heap-heap-hurray <input.gcdump> <output.heapsnapshot>");
            return;
        }

        var heapDump = new GCHeapDump(args[0]);
        using var output = new FileStream(args[1], FileMode.Create);
        using var writer = new Utf8JsonWriter(output);
        writer.WriteStartObject();

        writer.WriteStartObject("snapshot");

        writer.WriteStartObject("meta");
        writer.WriteStartArray("node_fields");
        writer.WriteStringValue("type");
        writer.WriteStringValue("name");
        writer.WriteStringValue("id");
        writer.WriteStringValue("self_size");
        writer.WriteStringValue("edge_count");
        writer.WriteStringValue("trace_node_id");
        writer.WriteStringValue("detachedness");
        writer.WriteEndArray();
        writer.WriteStartArray("node_types");
        writer.WriteStartArray();
        writer.WriteStringValue("hidden");
        writer.WriteStringValue("array");
        writer.WriteStringValue("string");
        writer.WriteStringValue("object");
        writer.WriteStringValue("code");
        writer.WriteStringValue("closure");
        writer.WriteStringValue("regexp");
        writer.WriteStringValue("number");
        writer.WriteStringValue("native");
        writer.WriteStringValue("synthetic");
        writer.WriteStringValue("concatenated string");
        writer.WriteStringValue("sliced string");
        writer.WriteStringValue("symbol");
        writer.WriteStringValue("bigint");
        writer.WriteStringValue("object shape");
        writer.WriteStringValue("wasm object");
        writer.WriteEndArray();
        writer.WriteStringValue("string");
        writer.WriteStringValue("number");
        writer.WriteStringValue("number");
        writer.WriteStringValue("number");
        writer.WriteStringValue("number");
        writer.WriteStringValue("number");
        writer.WriteEndArray();
        writer.WriteStartArray("edge_fields");
        writer.WriteStringValue("type");
        writer.WriteStringValue("name_or_index");
        writer.WriteStringValue("to_node");
        writer.WriteEndArray();
        writer.WriteStartArray("edge_types");
        writer.WriteStartArray();
        writer.WriteStringValue("context");
        writer.WriteStringValue("element");
        writer.WriteStringValue("property");
        writer.WriteStringValue("internal");
        writer.WriteStringValue("hidden");
        writer.WriteStringValue("shortcut");
        writer.WriteStringValue("weak");
        writer.WriteEndArray();
        writer.WriteStringValue("string_or_number");
        writer.WriteStringValue("node");
        writer.WriteEndArray();
        writer.WriteEndObject(); // /meta

        writer.WriteNumber("node_count", heapDump.MemoryGraph.NodeCount);

        int edgeCount = 0;
        Node nodeStorage = heapDump.MemoryGraph.AllocNodeStorage();
        for (NodeIndex nodeIndex = 0; nodeIndex < heapDump.MemoryGraph.NodeIndexLimit; nodeIndex++)
        {
            Node node = heapDump.MemoryGraph.GetNode(nodeIndex, nodeStorage);
            edgeCount += node.ChildCount;
        }
        writer.WriteNumber("edge_count", edgeCount);
        writer.WriteNumber("root_index", (long)heapDump.MemoryGraph.RootIndex);

        writer.WriteEndObject();

        writer.WriteStartArray("nodes");
        NodeType typeStorage = heapDump.MemoryGraph.AllocTypeNodeStorage();
        for (NodeIndex nodeIndex = 0; nodeIndex < heapDump.MemoryGraph.NodeIndexLimit; nodeIndex++)
        {
            Node node = heapDump.MemoryGraph.GetNode(nodeIndex, nodeStorage);
            MemoryNode? memoryNode = node as MemoryNode;
            NodeType type = heapDump.MemoryGraph.GetType(node.TypeIndex, typeStorage);
            if (type.ModuleName == null)
                writer.WriteNumberValue(9); // type: syntetic
            else
                writer.WriteNumberValue(3); // type: object
            writer.WriteNumberValue((long)node.TypeIndex);
            writer.WriteNumberValue(heapDump.MemoryGraph.GetAddress(nodeIndex));
            writer.WriteNumberValue(memoryNode?.Size ?? 0);
            writer.WriteNumberValue(node.ChildCount);
            writer.WriteNumberValue(0); // trace_node_id
            writer.WriteNumberValue(0); // detachedness
        }
        writer.WriteEndArray();

        writer.WriteStartArray("edges");
        for (NodeIndex nodeIndex = 0; nodeIndex < heapDump.MemoryGraph.NodeIndexLimit; nodeIndex++)
        {
            Node node = heapDump.MemoryGraph.GetNode(nodeIndex, nodeStorage);
            for (NodeIndex childIndex = node.GetFirstChildIndex(); 
                 childIndex != NodeIndex.Invalid;
                 childIndex = node.GetNextChildIndex())
            {
                writer.WriteNumberValue(2); // type: property
                writer.WriteNumberValue(0); // name_or_index
                writer.WriteNumberValue((long)childIndex * 7);
            }
        }
        writer.WriteEndArray();

        writer.WriteStartArray("trace_function_infos");
        writer.WriteEndArray();
        writer.WriteStartArray("trace_tree");
        writer.WriteEndArray();
        writer.WriteStartArray("samples");
        writer.WriteEndArray();
        writer.WriteStartArray("locations");
        writer.WriteEndArray();

        writer.WriteStartArray("strings");
        for (NodeTypeIndex typeIndex = 0;
             typeIndex < heapDump.MemoryGraph.NodeTypeIndexLimit;
             typeIndex++)
        {
            NodeType type = heapDump.MemoryGraph.GetType(typeIndex, typeStorage);
            writer.WriteStringValue(type.Name);
        }
        writer.WriteEndArray();

        writer.WriteEndObject();
    }
}