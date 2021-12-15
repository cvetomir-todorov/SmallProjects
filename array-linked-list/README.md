# Introduction

Implementation of the linked-list data structure via an array, which leads to an improved performance because of the fixed number of initial allocations, reduced GC pressure, cache locality.

## Representation

In the classic linked-list implementation we use a `Node` type which stores the value and has references to the previous and next items in the linked-list. The array implementation is similar but values are stored in an array. And we have 2 more arrays which store the indices to previous and next items.

Example:
```
AddLast("alpha"), AddLast("beta"), AddLast("gamma") with an initial capacity of 4 would result in:
linked-list:    ("alpha") <-> ("beta") <-> ("gamma")
items array:    ["alpha", "beta", "gamma", <default>]
previous array: [nil, 0, 1, <default>]
next array:     [1, 2, nil, <default>]
first:          0
last:           2
count:          3
```

## Mutations

* When an item is removed from the linked-list the item at the end is moved to the freed spot and previous/next indices are updated.
* That means adding new item would always be done at the end of the array.
* If the array is full a simple growing strategy creates new arrays with growth factor of 2 and copies the contents to them.

Remove example:
```
Initial state:
linked-list:    ("alpha") <-> ("beta") <-> ("gamma")
items array:    ["alpha", "beta", "gamma", <default>]
previous array: [nil, 0, 1, <default>]
next array:     [1, 2, nil, <default>]
first:          0
last:           2
count:          3

After Remove("alpha"):
linked-list:    ("beta") <-> ("gamma")
items array:    ["gamma", "beta", <default>, <default>]
previous array: [1, nil, <default>, <default>]
next array:     [nil, 0, <default>, <default>]
first:          1
last:           0
count:          2
```

## .NET interfaces supported

* `IEnumerable<T>`
* `IReadOnlyCollection<T>`
* `ICollection<T>`

## Benefits

* We do not allocate a new `Node` instance for each new item. Instead the value, and indices are stored in the already allocated arrays. The exception is when the arrays are already full which leads to growing them using a factor of `2`. Support for setting the initial capacity via the constructor allows optimizations which spare the unnecessary growing of the arrays.
* A fixed number of allocations - hopefully only 3 arrays and 1 main object leads to minimal pressure on the GC. We don't have a multitude of `Node` instances which would de-fragment memory and would need to be tracked, compacted and collected.
* Accessing the contents of the linked-list is sped-up because of cache locality. That is especially valid if the items in the linked-list are ordered consequently in the array since access pattern is sequential as well. Support for normalizing the internal structure would also allow for optimization related to this.