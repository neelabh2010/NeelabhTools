using System.Data;
using System.Reflection;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;

namespace NeelabhCoreTools.Tests;

// A compatibility test temporarily changes Json.NET's process-wide defaults.
[CollectionDefinition("Json.NET defaults", DisableParallelization = true)]
public sealed class JsonDefaultsCollection;

[Collection("Json.NET defaults")]
public class ExtraToolsTests
{
    [Fact]
    public void PublishedNamesDefaultsAndExtensionSignaturesAreRetained()
    {
        Assert.Null(typeof(ExtraTools).Namespace);
        foreach (var name in new[] { "IsTrue", "IsFalse" })
        {
            var method = typeof(ExtraTools).GetMethod(name)!;
            Assert.True(method.IsDefined(typeof(ExtensionAttribute)));
            Assert.Equal(new[] { "value", "trueValue", "falseValue" },
                method.GetParameters().Select(p => p.Name));
            Assert.Equal("", method.GetParameters()[2].DefaultValue);
        }
        Assert.Equal("no", false.IsFalse(trueValue: "no", falseValue: "yes"));
        Assert.Equal("", true.IsFalse(trueValue: "no"));
        Assert.Equal("yes", true.IsTrue(trueValue: "yes"));
        Assert.Equal("", false.IsTrue(trueValue: "yes"));

        var membership = typeof(ExtraTools).GetMethod("In")!;
        Assert.True(membership.IsDefined(typeof(ExtensionAttribute)));
        Assert.True(membership.GetParameters()[1].IsDefined(typeof(ParamArrayAttribute)));
        Assert.Contains(typeof(ExtraTools).GetMethods(), method =>
            method.Name == "Clone" && method.GetParameters().Length == 1 && method.IsGenericMethodDefinition);
        var mappings = typeof(ExtraTools).GetMethods().Where(method => method.Name == "TableToList").ToArray();
        Assert.Equal(2, mappings.Length);
        Assert.All(mappings, method =>
            Assert.Equal("dataRowToModelDelegate", method.GetParameters()[1].Name));
    }

    [Fact]
    public void LegacyMembershipHandlesNullsAndCollectionsAsSingleCandidates()
    {
        string? item = null;
        Assert.True(item.In((string?)null));
        Assert.False(item.In((string?[]?)null!));
        Assert.False(item.In());
        Assert.False("name".In("NAME"));
        object collection = new List<int> { 1, 2 };
        Assert.True(collection.In(collection));
        Assert.False(collection.In(new List<int> { 1, 2 }));
    }

    [Fact]
    public void CollectionMembershipUsesTheRequestedEqualityRules()
    {
        var items = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "NAME" };
        Assert.False("name".InCollection(items));
        Assert.True("name".InCollection(items, StringComparer.OrdinalIgnoreCase));
        Assert.False(1.InCollection(null!));
        Assert.True(((string?)null).InCollection(new string?[] { "name", null }));
    }

    [Fact]
    public void CollectionMembershipStopsAtTheFirstMatchWithoutMaterializing()
    {
        int visited = 0;
        IEnumerable<int> Items()
        {
            visited++;
            yield return 1;
            visited++;
            yield return 2;
            throw new InvalidOperationException("The matched sequence must not be read further.");
        }

        Assert.True(2.InCollection(Items()));
        Assert.Equal(2, visited);
    }

    [Fact]
    public void CloneCopiesNestedJsonStateWithoutSharingMutableValues()
    {
        var source = new CloneModel
        {
            Name = "original", Numbers = [1, 2], Ignored = "not serialized",
            Next = new CloneModel { Name = "child" }
        };
        var clone = ExtraTools.Clone(source);
        Assert.NotSame(source, clone);
        Assert.NotSame(source.Numbers, clone.Numbers);
        Assert.NotSame(source.Next, clone.Next);
        Assert.Equal(source.Numbers, clone.Numbers);
        Assert.Equal("child", clone.Next!.Name);
        Assert.Equal("default", clone.Ignored);
        clone.Numbers.Add(3);
        clone.Next.Name = "changed";
        Assert.Equal(new[] { 1, 2 }, source.Numbers);
        Assert.Equal("child", source.Next.Name);
        Assert.Equal(42, ExtraTools.Clone(42));
        Assert.Null(ExtraTools.Clone<CloneModel?>(null));
    }

    [Fact]
    public void CloneSettingsAreAppliedToBothSerializationAndDeserialization()
    {
        var converter = new ModelConverter();
        var settings = new JsonSerializerSettings { Converters = { converter } };
        var clone = ExtraTools.Clone(new CloneModel { Name = "encoded" }, settings);
        Assert.Equal("encoded", clone.Name);
        Assert.Equal(1, converter.Writes);
        Assert.Equal(1, converter.Reads);
    }

    [Fact]
    public void LegacyCloneStillHonorsGlobalSettingsIncludingNullConversion()
    {
        var originalSettings = JsonConvert.DefaultSettings;
        try
        {
            var converter = new ModelConverter();
            JsonConvert.DefaultSettings = () => new JsonSerializerSettings { Converters = { converter } };
            Assert.Equal("global", ExtraTools.Clone(new CloneModel { Name = "global" }).Name);
            Assert.Equal("converted null", ExtraTools.Clone<CloneModel?>(null)!.Name);
            Assert.True(converter.Reads >= 2);
        }
        finally { JsonConvert.DefaultSettings = originalSettings; }
    }

    [Fact]
    public void CloneCanPreserveCyclesOnlyWhenExplicitlyConfigured()
    {
        var source = new CloneModel { Name = "cycle" };
        source.Next = source;
        Assert.Throws<JsonSerializationException>(() => ExtraTools.Clone(source));
        var clone = ExtraTools.Clone(source, new JsonSerializerSettings
        {
            PreserveReferencesHandling = PreserveReferencesHandling.Objects
        });
        Assert.NotSame(source, clone);
        Assert.Same(clone, clone.Next);
    }

    [Fact]
    public void TryCloneReturnsJsonErrorsAndDoesNotSwallowUnrelatedConverterErrors()
    {
        var source = new CloneModel();
        source.Next = source;
        Assert.False(ExtraTools.TryClone(source, out var failed, out var error));
        Assert.Null(failed);
        Assert.IsType<JsonSerializationException>(error);
        Assert.True(ExtraTools.TryClone(new CloneModel { Name = "copied" }, out var clone, out error));
        Assert.Equal("copied", clone.Name);
        Assert.Null(error);
        Assert.True(ExtraTools.TryClone<CloneModel?>(null, out var empty, out error));
        Assert.Null(empty);
        Assert.Null(error);

        var failure = new InvalidOperationException("Custom converter failure");
        var settings = new JsonSerializerSettings { Converters = { new ThrowingConverter(failure) } };
        var actual = Assert.Throws<InvalidOperationException>(() =>
            ExtraTools.TryClone(source, out _, out _, settings));
        Assert.Same(failure, actual);
    }

    [Fact]
    public void TableMappingRetainsMissingInputAndFailedResultSemantics()
    {
        using var table = CreateTable();
        Func<DataRow, int> mustNotRun = _ => throw new InvalidOperationException("Unexpected mapping");
        Assert.Null(ExtraTools.TableToList((DataTable)null!, mustNotRun));
        Assert.Null(ExtraTools.TableToList<int>(table, null!));
        Assert.Null(ExtraTools.TableToList((ResultInfo)null!, mustNotRun));
        Assert.Null(ExtraTools.TableToList(new ResultInfo(), mustNotRun));
        Assert.Null(ExtraTools.TableToList(new ResultInfo { HasError = true, Table = table }, mustNotRun));
        Assert.Null(ExtraTools.TableToList<int>(new ResultInfo { Table = table }, null!));
    }

    [Fact]
    public void BothTableOverloadsMapEachRowOnceAndInOrder()
    {
        using var table = CreateTable();
        table.Rows.Add(3);
        table.Rows.Add(1);
        table.Rows.Add(2);
        int calls = 0;
        int Map(DataRow row) { calls++; return (int)row["Id"] * 10; }
        Assert.Equal(new[] { 30, 10, 20 }, ExtraTools.TableToList(table, Map));
        Assert.Equal(3, calls);
        calls = 0;
        Assert.Equal(new[] { 30, 10, 20 }, ExtraTools.TableToList(new ResultInfo { Table = table }, Map));
        Assert.Equal(3, calls);
        Assert.Equal(new[] { 3, 1, 2 }, table.Rows.Cast<DataRow>().Select(row => (int)row["Id"]));
    }

    [Fact]
    public void EmptyTablesReturnIndependentEmptyListsWithoutInvokingTheMapper()
    {
        using var table = CreateTable();
        Func<DataRow, int> mustNotRun = _ => throw new InvalidOperationException("Unexpected mapping");
        var direct = ExtraTools.TableToList(table, mustNotRun);
        var wrapped = ExtraTools.TableToList(new ResultInfo { Table = table }, mustNotRun);
        Assert.Empty(direct);
        Assert.Empty(wrapped);
        Assert.NotSame(direct, wrapped);
    }

    [Fact]
    public void MappingFailuresPreserveTheOriginalExceptionAndStopEnumeration()
    {
        using var table = CreateTable();
        table.Rows.Add(1);
        table.Rows.Add(2);
        var failure = new InvalidOperationException("Invalid row");
        int calls = 0;
        int Map(DataRow row) { calls++; throw failure; }
        Assert.Same(failure, Assert.Throws<InvalidOperationException>(() => ExtraTools.TableToList(table, Map)));
        Assert.Equal(1, calls);
        calls = 0;
        Assert.Same(failure, Assert.Throws<InvalidOperationException>(() =>
            ExtraTools.TableToList(new ResultInfo { Table = table }, Map)));
        Assert.Equal(1, calls);
    }

    private static DataTable CreateTable()
    {
        var table = new DataTable();
        table.Columns.Add("Id", typeof(int));
        return table;
    }

    public sealed class CloneModel
    {
        public string Name { get; set; } = "";
        public List<int> Numbers { get; set; } = [];
        public CloneModel? Next { get; set; }
        [JsonIgnore] public string Ignored { get; set; } = "default";
    }

    private sealed class ModelConverter : JsonConverter<CloneModel>
    {
        public int Reads { get; private set; }
        public int Writes { get; private set; }

        public override void WriteJson(JsonWriter writer, CloneModel? value, JsonSerializer serializer)
        {
            Writes++;
            writer.WriteValue("model:" + value!.Name);
        }

        public override CloneModel? ReadJson(JsonReader reader, Type objectType, CloneModel? existingValue,
            bool hasExistingValue, JsonSerializer serializer)
        {
            Reads++;
            if (reader.TokenType == JsonToken.Null) return new CloneModel { Name = "converted null" };
            string text = (string)reader.Value!;
            if (!text.StartsWith("model:", StringComparison.Ordinal)) throw new JsonSerializationException("Invalid encoding");
            return new CloneModel { Name = text[6..] };
        }
    }

    private sealed class ThrowingConverter(Exception failure) : JsonConverter<CloneModel>
    {
        public override void WriteJson(JsonWriter writer, CloneModel? value, JsonSerializer serializer) => throw failure;
        public override CloneModel? ReadJson(JsonReader reader, Type objectType, CloneModel? existingValue,
            bool hasExistingValue, JsonSerializer serializer) => throw failure;
    }
}
