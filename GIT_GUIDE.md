# Uploading Nover to GitHub

1. On GitHub, create a new public repository named `nover` without README, .gitignore, or license
   (these files already exist in the project).
2. Open a terminal in the project folder and run:

```
git init
git add .
git status
```

3. Check the output of `git status`. It must NOT list:
   `nover_nutzer.json`, `nover_daten.json`, `nover_sitzungen.json`, `*.log`, `*.pfx`, `bin/`, `obj/`, `dist/`.
   If any of them appear, do not commit. Check `.gitignore` first.

4. Then run:

```
git commit -m "Initial version of Nover"
git branch -M main
git remote add origin https://github.com/YOUR-USERNAME/nover.git
git push -u origin main
```

Git asks for authentication. Use a GitHub personal access token instead of your password.
