using System;
using System.Collections.Generic;
using System.Text;

namespace Janrain.OpenId.Session
{
    public interface ISessionState
    {
        // Summary:
        //     Gets or sets a session value by numerical index.
        //
        // Parameters:
        //   index:
        //     The numerical index of the session value.
        //
        // Returns:
        //     The session-state value stored at the specified index.
        object this[int index] { get; set; }
        //
        // Summary:
        //     Gets or sets a session value by name.
        //
        // Parameters:
        //   name:
        //     The key name of the session value.
        //
        // Returns:
        //     The session-state value with the specified name.
        object this[string name] { get; set; }
        //
        // Summary:
        //     Adds a new item to the session-state collection.
        //
        // Parameters:
        //   name:
        //     The name of the item to add to the session-state collection.
        //
        //   value:
        //     The value of the item to add to the session-state collection.
        void Add(string name, object value);
        //
        // Summary:
        //     Removes all keys and values from the session-state collection.
        void Clear();
        //
        // Summary:
        //     Deletes an item from the session-state collection.
        //
        // Parameters:
        //   name:
        //     The name of the item to delete from the session-state collection.
        void Remove(string name);
        //
        // Summary:
        //     Gets the number of items in the session-state collection.
        //
        // Returns:
        //     The number of items in the collection.
        int Count { get; }
    }
}
