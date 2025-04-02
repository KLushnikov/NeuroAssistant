using LibGit2Sharp;
using System;

namespace NeuroAssistant.Git
{
    public class GitDiffHelper
    {
        /// <summary>
        /// Retrieves the git diff between the working tree and the last commit
        /// </summary>
        public string GetDiff(string repoPath = null)
        {
            try
            {
                // 1. Automatically discover repository if path is not provided
                if (string.IsNullOrEmpty(repoPath))
                {
                    repoPath = Repository.Discover(Environment.CurrentDirectory);
                }

                if (string.IsNullOrEmpty(repoPath))
                {
                    throw new ArgumentException("Git repository not found");
                }

                // 2. Open the repository
                using (var repo = new Repository(repoPath))
                {
                    // 3. Get diff between working tree and HEAD
                    var patch = repo.Diff.Compare<Patch>(
                        repo.Head.Tip.Tree,
                        DiffTargets.WorkingDirectory
                    );

                    return patch.Content;
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to get git diff", ex);
            }
        }

        /// <summary>
        /// Gets staged diff (between index and HEAD)
        /// </summary>
        public string GetStagedDiff(string repoPath = null)
        {
            try
            {
                // 1. Automatically discover repository if path is not provided
                if (string.IsNullOrEmpty(repoPath))
                {
                    repoPath = Repository.Discover(Environment.CurrentDirectory);
                }

                if (string.IsNullOrEmpty(repoPath))
                {
                    throw new ArgumentException("Git repository not found");
                }

                // 2. Open the repository
                using (var repo = new Repository(repoPath))
                {
                    // 3. Compare index with HEAD
                    var patch = repo.Diff.Compare<Patch>(
                        repo.Head.Tip.Tree,
                        DiffTargets.Index
                    );

                    return patch.Content;
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to get staged diff", ex);
            }
        }
    }
}
