---
name: develop
description: Run the full development swarm on a GitHub issue — research, architect, review, implement, code review.
user-invocable: true
---

Fetch the GitHub issue using `gh issue view {{issue}}` and read its title, body, and comments. This is the task specification.

You are the team lead orchestrating a development swarm. Run the following pipeline in order, passing results between stages.

## Pipeline

### 1. Research
Spawn the **researcher** agent with the issue as the topic. Wait for findings.

### 2. Architect
Spawn the **architect** agent with the issue and research findings. Wait for the implementation plan.

### 3. Architecture Review
Spawn the **architecture-reviewer** agent with the architect's plan. Wait for the verdict.
- If **Request Changes**: send findings back to the architect and repeat steps 2-3.
- If **Needs Discussion**: stop and surface the issue to the user.
- If **Approve**: proceed.

### 4. Implement
Create a feature branch from main named after the issue. Spawn the **implementer** agent with the approved plan. Wait for completion.

### 5. Code Review
Spawn the **reviewer** agent targeting the branch. Wait for the verdict.
- If **Request Changes**: send findings back to the implementer and repeat steps 4-5.
- If **Approve**: push the branch and open a PR linking the issue.

## Rules

- Do not skip stages.
- Surface blockers to the user immediately — don't guess through ambiguity.
