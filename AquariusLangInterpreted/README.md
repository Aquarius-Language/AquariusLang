Stuffs implemented from "Writing an Interpreter in Go" book:
    ~ page 189.

<h3>IMPORTANT!:</h3>
- Statement and expression nodes under AbstractSyntaxTree.cs needs to be class. 
Because they implement from IStatement and IExpression interfaces. Structs are
bad for type casting (polymorphism).
- HashKey MUST either be struct type for being stack variables, or they need to override Equals() and GetHashCode(),
so they can be used as dictionary keys. Otherwise, it'll not work as dictionary keys. (Maybe because class instances are pointers to heap?)
Extra advantage of this: since HashKey doesn't have complicated types nor polymorphic members, it also might boost performance.

<h3>FEATURES TO IMPLEMENT:</h3>

1. While loop.
2. Floating point numbers.
3. Use "Visitor pattern" to replace "type checking using switch case" under Evaluator.Eval(). (for performance enhancement)
4. Make numbers available as part of identifiers, such as (let hello123 = "Hello").
5. Implement else if (elif)...
6. Value re-assignment to array and hashmap's elements.
7. pass array and hashmap by reference to functions.
8. Add "for loop example unit testing" when the problems for for loop are fixed.
9. Make for loop still work even when no few statements in parenthesis. ex. for(; i < 10; i+=1){}
10. Binary and, or (|, &)
11. String concatenate with int.
12. *=, /=.
13. Unit testing for break statement, once inner and outer variables' scope problem's fixed.
14. Prevent re-declaring variables that already exist.
15. Importing other files as modules.
16. exit() to exit application.
17. Make more types (even custom types) available as hashmap keys.
18. NullObj type values should be printable. Therefore, implement a void type to tell which are not printable.
19. Make variables not re-declarable within same environment that owns it.
20. Design cross-platform DevOps-related libraries.
21. Design some new syntax combined with libffi library to make calling C-API possible.
22. Some objects have array ([]) members. But most of their instantiation are passed from list converted to array.
    Maybe, just change them all to lists, might reduce the time cost of data structure conversion.

<h3>FINISHED IMPLEMENTING:</h3>

1. Variable re-assignment. Added to Evaluator.assignVariableVal().
2. For loop. Added to Evaluator.evalForLoopLiteral() and.parseForLoopLiteral().  (global and local issues not completely done yet)
3. ~~String keys are saved as hashcode/int (.GetHashCode()) in dictionary for Hashmap type in Aquarius lang.
   The hashcode version of the string and int values might happen to have collisions of same value. To fix
   that, can use linked lists as values for the hashmap. (learnt from Algorithm course in university)~~
   Turns out there's no need to do that, since HashKey instance contains "type" member as string. Therefore,
   the hashmap in the language can differentiate between two instances with different types but same hash int values.
4. Implement ||, &&, <=, >=...
5. "return" out of function from loop. (added inside Evaluator.evalForLoopLiteral())
6. "break" statement out of loops. (added inside Evaluator.evalForLoopLiteral())
7. Implement "outer" and "local" variables for environments. (by adding Environment.Create(), Environment.owned etc...)
8. Ignore comments. '#' for single-line comment and "##" for multi-line comments. (added into Lexer.skipComments())
9. Print out the illegal token for "No prefix parse function for ILLEGAL found." (added by setting inside Parser.noPrefixFnParse())

<h3>BUGS TO FIX:</h3>

1. Fix no error printed out for "for (let i = 0; i < len(array); i+=1) {print(array[i])}" if "array" array variable is not declared.
2. Fix "++" typo cannot be caught in debug log. (ex. for (let i = 0; i < 5; i++)...)


<h3>BUGS FIXED:</h3>

1. (AquariusDeskInterpretedREPL/examples/var_scope.aqua prints an extra "null" at the end. Figure out why.) - "print()" originally 
   returns NullObj instead of null. Changed to null. 

<h3>SUGGESTIONS AND NOTES WHEN IMPLEMENTING NEW FEATURES:</h3>

- Be VERY CAREFUL about when to call nextTokens(). I missed few nextTokens() call when I was implementing for loop parsing.
  But they seem to get fixed when I added those function calls.
- Sometimes when adding new operators but it's not showing up in search for prefix/infix callbacks, make sure if they're being 
  referenced under Parser.precedencesMap.
- List.Append() doesn't add item to list. It returns new instance of list with added item. Use .Add() instead.

<h3>POSSIBLE FUTURE FEATURES TO PUT INTO CONSIDERATIONS:</h3>

- Embedding Git functionality as standard library.
- Interop with multiple languages.
- Libgccjit or WASI or compiling to DLL (possibly using Zigs) for JIT compiling.
- Maybe make a wrapper and library for Wicked Engine?
- Wrapper for PyTorch?
- Calculus math calculations as part of language's operators?
- Wrapper for desktop GUI library.
- Maybe wrapper for sokol.h: https://github.com/floooh/sokol, a cross-platform library.
- Or maybe wrapper for Raylib.
- Or maybe Magnum Graphics: https://magnum.graphics/showcase/
- Bindings for SFML/SDL2.
- Bindings for MonoGame. (might actually be a good idea) MonoGame course: https://www.youtube.com/watch?v=r5dM0_J7KuY&list=PLV27bZtgVIJqoeHrQq6Mt_S1-Fvq_zzGZ
