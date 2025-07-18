
;* LISTCHAR.LSP - Draws a table with all 256 ASCII-numbers followed by the corresponding character.
;                 Load and run this lisp in an empty drawing, with the Textstyle to investigate set current.
;                 Undefined numbers do result into a queastion mark character.
;                 (But beware, the real question mark character also does)
;
;                 Written by M. Moolhuysen

(defun C:LISTCHAR ()
  (setq cnt 1)
  (while (< cnt 256)
    (command "_TEXT" "_J" "_R"
                     (list (* 20 (fix (/ (1- cnt) 51)))
                           (* -2.7 (rem (1- cnt) 51)))
                     1.8 0 (itoa cnt))
    (command "_TEXT" (list (+ 1.5 (* 20 (fix (/ (1- cnt) 51))))
                           (* -2.7 (rem (1- cnt) 51)))
                     1.8 0 (chr cnt))
    (setq cnt (1+ cnt))
  )
  (command "_ZOOM" "_E")
  (princ)
)