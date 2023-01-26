Stuffs implemented from "Writing an Interpreter in Go" book:
    ~ page 189.

<h3>IMPORTANT!:</h3>
- Statement and expression nodes under AbstractSyntaxTree.cs needs to be class. 
Because they implement from IStatement and IExpression interfaces. Structs are
bad for type casting (polymorphism).
- HashKey MUST either be struct type for being stack variables, or they need to override Equals() and GetHashCode(),
so they can be used as dictionary keys. Otherwise, it'll not work as dictionary keys. (Maybe because class instances are pointers to heap?)
Extra advantage of this: since HashKey doesn't have complicated types nor polymorphic members, it also might boost performance.

<h3>SUGGESTIONS AND NOTES WHEN IMPLEMENTING NEW FEATURES:</h3>

- Be VERY CAREFUL about when to call nextTokens(). I missed few nextTokens() call when I was implementing for loop parsing.
  But they seem to get fixed when I added those function calls.
- Sometimes when adding new operators but it's not showing up in search for prefix/infix callbacks, make sure if they're being 
  referenced under Parser.precedencesMap.
- List.Append() doesn't add item to list. It returns new instance of list with added item. Use .Add() instead.

<h3>POSSIBLE FUTURE FEATURES TO PUT INTO CONSIDERATIONS:</h3>

- Embedding Git functionality as standard library.
- Interop with multiple languages.
- Libgccjit or WASI or compiling to DLL (possibly using Zigs? Or maybe Cython?) for JIT or AOT compiling.
- Or maybe just compile to Julia and run it using embedded Julia. Since Julia is very easy to embed.
- Maybe make a wrapper and library for Wicked Engine?
- Wrapper for PyTorch?
- Calculus math calculations as part of language's operators?
- Wrapper for desktop GUI library.
- Maybe wrapper for sokol.h: https://github.com/floooh/sokol, a cross-platform library.
- Or maybe wrapper for Raylib.
- Or maybe Magnum Graphics: https://magnum.graphics/showcase/
- Bindings for SFML/SDL2.
- Bindings for MonoGame. (might actually be a good idea) MonoGame course: https://www.youtube.com/watch?v=r5dM0_J7KuY&list=PLV27bZtgVIJqoeHrQq6Mt_S1-Fvq_zzGZ

NOTES AND SPECIALS ABOUT THIS LANGUAGE

- If a variable/identifier is declared int/float/double type, but right operand's also number but not the same type, the right operand's value gets casted into the same type before assigning.
  (basically, the variable/identifier's number type keeps the same)
