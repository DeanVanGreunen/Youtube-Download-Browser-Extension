set "string=%1"
set "link=%string:~3%"
python -m youtube_dl --restrict-filenames --ignore-errors -x --audio-format mp3 -o "%(title)s.%(ext)s" %link%