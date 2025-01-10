using System;
using System.Collections.Generic;
using System.Linq;

namespace FlowRules.Engine;

/// <summary>
/// Implementation of a nested dictionary.
/// Allows the user to create a dictionary of the form lookup["key1"]["key2"]["key3"].
/// Attempting to access a key which does not exist results in a default value being returned, rather than an exception being thrown.
/// </summary>
/// <typeparam name="TKey">The dictionary key type.</typeparam>
/// <typeparam name="TValue">The dictionary value type.</typeparam>
public class NestedLookup<TKey, TValue>
    where TKey : notnull
{
    private readonly Dictionary<TKey, NestedLookup<TKey, TValue>> _data = [];
    private readonly Func<TValue?>? _defaultValueFactory;
    private TValue? _value;

    /// <summary>
    /// Initializes a new instance of the <see cref="NestedLookup{TKey, TValue}"/> class.
    /// </summary>
    /// <param name="items">The items to add to the dictionary.</param>
    /// <param name="defaultValueFactory">A factory to create default values if the key was not found.</param>
    /// <exception cref="ArgumentNullException">An exception thrown if the items argument is null.</exception>
    /// <exception cref="ArgumentException">An exception thrown if the items argument is empty.</exception>
    public NestedLookup(
        IEnumerable<(IEnumerable<TKey> Keys, TValue Value)> items,
        Func<TValue>? defaultValueFactory = null)
        : this(defaultValueFactory)
    {
        ArgumentNullException.ThrowIfNull(items);

        foreach ((IEnumerable<TKey> keys, TValue value) in items.ToList())
        {
            if (keys?.Any() != true)
            {
                throw new ArgumentException("Keys cannot be null or empty.", nameof(items));
            }

            Add(keys, value);
        }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="NestedLookup{TKey, TValue}"/> class.
    /// </summary>
    /// <param name="defaultValueFactory">A factory to create default values if the key was not found.</param>
    public NestedLookup(Func<TValue?>? defaultValueFactory = null)
    {
        _defaultValueFactory = defaultValueFactory ?? (() => default);
        _value = _defaultValueFactory();
    }

    /// <summary>
    /// Implicit cast operator to return the value as an integer.
    /// </summary>
    /// <param name="lookup">The lookup value.</param>
    public static implicit operator int(NestedLookup<TKey, TValue> lookup) => lookup.As<int>();

    /// <summary>
    /// Implicit cast operator to return the value as a double.
    /// </summary>
    /// <param name="lookup">The lookup value.</param>
    public static implicit operator double(NestedLookup<TKey, TValue> lookup) => lookup.As<double>();

    /// <summary>
    /// Implicit cast operator to return the value as a decimal.
    /// </summary>
    /// <param name="lookup">The lookup value.</param>
    public static implicit operator decimal(NestedLookup<TKey, TValue> lookup) => lookup.As<decimal>();

    /// <summary>
    /// Implicit cast operator to return the value as a string.
    /// </summary>
    /// <param name="lookup">The lookup value.</param>
    public static implicit operator string?(NestedLookup<TKey, TValue> lookup) => lookup.AsString;

    /// <summary>
    /// Implicit cast operator to return the value as a boolean.
    /// </summary>
    /// <param name="lookup">The lookup value.</param>
    public static implicit operator bool(NestedLookup<TKey, TValue> lookup) => lookup.As<bool>();

    /// <summary>
    /// Indexer for accessing an item in the dictionary.
    /// </summary>
    /// <param name="key">The key to index.</param>
    /// <returns>The item or a default value.</returns>
    /// <exception cref="ArgumentNullException">An exception thrown if the key is null.</exception>
    public NestedLookup<TKey, TValue> this[TKey key]
    {
        get
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key), "Key cannot be null.");
            }

            if (!_data.TryGetValue(key, out NestedLookup<TKey, TValue>? nested))
            {
                nested = new NestedLookup<TKey, TValue>(_defaultValueFactory);
                _data[key] = nested;
            }

            return nested;
        }
    }

    /// <summary>
    /// Checks if a key is defined.
    /// </summary>
    /// <param name="key">The key.</param>
    /// <returns>True if the key is defined, otherwise false.</returns>
    /// <exception cref="ArgumentNullException">An exception thrown if the key is null.</exception>
    public bool IsDefined(TKey key)
    {
        if (key == null)
        {
            throw new ArgumentNullException(nameof(key), "Key cannot be null.");
        }

        return _data.TryGetValue(key, out NestedLookup<TKey, TValue>? _);
    }

    private string? AsString
    {
        get
        {
            return _value as string;
        }
    }

    private T As<T>()
        where T : struct
    {
        if (_value == null)
        {
            return default;
        }

        return (T)Convert.ChangeType(_value, typeof(T));
    }

    private void Add(IEnumerable<TKey> keys, TValue value)
    {
        if (keys?.Any() != true)
        {
            throw new ArgumentException("Keys cannot be null or empty.", nameof(keys));
        }

        NestedLookup<TKey, TValue> current = this;
        foreach (TKey key in keys)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(keys), "One of the keys is null.");
            }

            current = current[key];
        }

        current.SetValue(value);
    }

    private void SetValue(TValue value)
    {
        if (value == null)
        {
            throw new ArgumentNullException(nameof(value), "Value cannot be null.");
        }

        _value = value;
    }
}
