# For Maintainers

This branch is forked from a subtree of https://github.com/Unity-Technologies/Graphics . If you are attempting to merge upstream changes, the first time after creating your local repo you'll need to add the unity repo as a remote named "upstream" and create a new branch cloned from the 2022.2/staging branch of the unity repo.

```
// Add unity's repo as a remote named "upstream"
git remote add upstream https://github.com/Unity-Technologies/Graphics.git
git fetch upstream

// checkout the 2022.2 staging (stable basically) branch into upstream-staging branch, disabling some lfs checks because lfs is dumb
git lfs install --skip-smudge // Disable smudge so it doesn't fail on not being able to download LFS files
git checkout -f -b upstream-staging upstream/2022.2/staging
git lfs pull
git lfs install --force //re-instate smudge
```

To merge changes use subtree to filter the relevant changesets relating to the RP core package folder and merge them into the upstream-core branch. Then merge upstream-core into master-2022.2

```
// switch to upstream-staging which we will be filtering
git checkout upstream-staging

// get latest commits from unity repo
git pull

// Filter main repo history for commits relating to the core folder, merge them into upstream-core, and switch to upstream-core
git subtree split --prefix=Packages/com.unity.render-pipelines.core --onto upstream-core -b upstream-core	

// push changes to upstream-core
git push

// Switch back to master-2022.2, and merge the new commits from upstream-core
git checkout master-2022.2
git merge upstream-core
```
