# Scheme implementation in C# ![](https://github.com/syohex/simple-scheme-sharp/workflows/CI/badge.svg)

This is based on http://peter.michaux.ca/index#Scheme

## How to use

### REPL

Run `SimpleScheme` project without arguments

```
% dotnet run --project SimpleScheme
Welcome to SimpleScheme
> 1
1
> 2
2
> (+ 1 2 3)
6
> (length '(1 2 3 4 5))
5
>
```

### Load file

Pass file name as command line arguments

```
% cat example/fizzbuzz.scm
(define (fizzbuzz1 i n)
  (if (> i n)
      ()
      (cons (cond ((and (= (mod i 3) 0) (= (mod i 5) 0)) "fizzbuzz")
		  ((= (mod i 5) 0) "buzz")
		  ((= (mod i 3) 0) "fizz")
		  (#t i))
	    (fizzbuzz1 (+ i 1) n))))

(define (fizzbuzz n)
  (fizzbuzz1 1 n))

(write (fizzbuzz 15))
(write-char #\newline)

% dotnet run --project SimpleScheme example/fizzbuzz.scm
(1 2 "fizz" 4 "buzz" "fizz" 7 8 "fizz" "buzz" 11 "fizz" 13 14 "fizzbuzz")
```
