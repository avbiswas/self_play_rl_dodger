# Self-Play RL Dodger

2D Unity ML-Agents game where two players dodge, jump, crouch, dash, and shoot bullets. One side can be human controlled, or both sides can run trained policies.

The main training setup is PPO self-play with the `BulletShooter` behavior.

https://github.com/user-attachments/assets/1708ed58-ab74-49ba-beff-2e5dad0a0040

## Support

If you find this helpful, consider supporting on Patreon!

[<img src="https://c5.patreon.com/external/logo/become_a_patron_button.png" alt="Become a Patron!" width="200">](https://www.patreon.com/NeuralBreakdownwithAVB)

## Model

- **Included model:** `Assets/RLAgents/results/env_sp11/BulletShooter/BulletShooter-3101431.onnx`
- **Main config:** `Assets/RLAgents/config/selfPlay.yaml`
- **Network:** PPO self-play policy, feed-forward MLP, 3 hidden layers, 512 units per layer, no recurrent memory.

## What is Self-Play?

Self-play is Machine Learning technique where the AI plays against previous versions of itself till it gets better! During training, we save older policy snapshots and re-use them as opponents for our current model. This updates policies to be diversely capable against a variety of strategies.

## How to play locally

Open the project in Unity, then go to `File > Build Settings`.

Select `WebGL`, include `Scenes/MainMenu` and `Scenes/GameEnv`, then build to a folder such as `Builds-WebGL`.

For easiest local testing, set `Player Settings > Publishing Settings > Compression Format` to `Disabled`.

After the build finishes, serve the folder locally:

```bash
cd Builds-WebGL
python3 -m http.server 8080
```

Then open:

```text
http://localhost:8080
```

Play!!


## How to train

To train headlessly, first make a standalone Unity build. In Unity, use `File > Build Settings`, choose `Windows, Mac, Linux`, include `MainMenu` and `GameEnv`, then build to a local path such as `Builds/RLBuild.app`.

Then run ML-Agents against that build:

```bash
mlagents-learn Assets/RLAgents/config/selfPlay.yaml --run-id env_sp_new --env Builds/RLBuild.app --num-envs 8 --no-graphics
```

Training outputs go under `Assets/RLAgents/results/`. Keep only the final `.onnx` model and its `.meta` file in git.



## Environment

- **Initial state:** both players reset to their spawn positions with zero velocity, alive state, dash available, crouch off, and bullets reset.

- **State space:** player position, velocity, crouch state, dash state, dash cooldown, shot cooldown, plus ray perception sensors for nearby bullets, walls, ground, and opponent.

- **Action space:** five discrete controls: move left/right/idle, jump, crouch, dash, and shoot. Shooting is masked while crouching.

- **Reward space:** death gives `-1`, winning gives `+1`, and bullet dodge reward is currently `0`. Env is super sparse, which is great for self-play!

- **Terminal state:** a player dies by bullet, falling, or inactivity. The dead player ends the episode and the surviving player gets the win reward.

## Devlog

Playlist: https://www.youtube.com/playlist?list=PLGXWtN1HUjPdoJwzrCmfVCtOY2GN2kzEb

These were some of my first videos, so the audio is a bit weak.

## Training config

The shared policy run used PPO with self-play:

- **Learning rate:** `0.0002` - small enough to keep self-play training stable.
- **Batch size:** `2048` - large batch for smoother PPO updates.
- **Buffer size:** `20480` - collects enough experience before each update.
- **Time horizon:** `256` - lets rewards connect to longer dodge/shoot sequences.
- **Self-play window:** `10` - keeps a pool of older opponents.
- **Swap steps:** `5,000` - changes opponents often enough to avoid overfitting.
- **Opponent mix:** `0.5` latest model ratio - half recent opponent, half older snapshots.

## Controls (in human vs AI mode)

- **Move:** `A` / `D` or left / right
- **Jump:** space
- **Crouch:** down / `S`
- **Dash:** dash input from the Unity input map
- **Shoot:** `K`

## Note

This project predates vibe-coding. I made it to learn Unity and build something cool with self-play. There may be superfluous files, old experiments, and dead code.
