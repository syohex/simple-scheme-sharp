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
