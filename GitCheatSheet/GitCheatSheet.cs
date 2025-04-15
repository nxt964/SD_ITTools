using ToolInterface;

namespace GitCheatSheet;

public class GitCheatSheetTool : ITool
{
    public string Name => "Git CheatSheet";
    public string Description => "A quick reference guide of common Git commands.";
    public string Category => "Development";

    public object Execute(object? input) => "No input required for Git Cheat Sheet.";

    public string GetUI()
    {
        return @"
<div class='container py-5 mx-auto' style='max-width: 900px;'>
    <div class='header mb-4'>
        <h1 class='text-start m-0'>Git CheatSheet</h1>
        <div class='separator my-2' style='width: 300px; height: 1.5px; opacity: 0.3; background: #a1a1a1'></div>
        <p class='text-start text-muted mb-0'>A quick reference guide of common Git commands.</p>
    </div>

    <div class='card shadow-sm p-4'>
        <h5 class='fw-bold mb-3'>🔧 Setup</h5>
        <pre><code>git config --global user.name ""Your Name""
git config --global user.email ""your@example.com""</code></pre>

        <h5 class='fw-bold mt-4 mb-3'>📦 Initialize & Clone</h5>
        <pre><code>git init
git clone &lt;repository_url&gt;</code></pre>

        <h5 class='fw-bold mt-4 mb-3'>💾 Staging & Commit</h5>
        <pre><code>git status
git add &lt;file&gt;
git add .          # Add all files
git commit -m ""Your message""</code></pre>

        <h5 class='fw-bold mt-4 mb-3'>🌿 Branching</h5>
        <pre><code>git branch
git branch &lt;branch_name&gt;
git checkout &lt;branch_name&gt;
git checkout -b &lt;new_branch&gt;</code></pre>

        <h5 class='fw-bold mt-4 mb-3'>🔁 Merge & Rebase</h5>
        <pre><code>git merge &lt;branch&gt;
git rebase &lt;branch&gt;</code></pre>

        <h5 class='fw-bold mt-4 mb-3'>🚀 Push & Pull</h5>
        <pre><code>git remote -v
git remote add origin &lt;url&gt;
git push -u origin &lt;branch&gt;
git pull origin &lt;branch&gt;</code></pre>

        <h5 class='fw-bold mt-4 mb-3'>⏪ Undo</h5>
        <pre><code>git restore &lt;file&gt;              # Discard changes
git reset HEAD &lt;file&gt;           # Unstage file
git checkout -- &lt;file&gt;          # Legacy discard
git revert &lt;commit_hash&gt;</code></pre>

        <h5 class='fw-bold mt-4 mb-3'>📄 Log & Diff</h5>
        <pre><code>git log
git log --oneline
git diff
git diff &lt;commit1&gt; &lt;commit2&gt;</code></pre>

        <h5 class='fw-bold mt-4 mb-3'>🔍 Stash</h5>
        <pre><code>git stash
git stash list
git stash apply
git stash drop</code></pre>
    </div>
</div>";
    }

    public void Stop() { }
}
