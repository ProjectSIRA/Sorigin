<script lang="ts" context="module">
  import type User from "$lib/types/user";
  import { SORIGIN_URL } from "$lib/utils/env";
  import { userByUsername, getUsernames } from "$lib/sorigin";

  export async function load({ page }) {
    const userResponse = await userByUsername(page.params.username);

    if (userResponse.data) {
      return { props: { user: userResponse.data } };
    }
    return {
      error: new Error(userResponse.errorMessage),
    };
  }
</script>

<script lang="ts">
  import SocialButtonGroup from "$lib/buttons/SocialButtonGroup.svelte";
  import { sessionStore } from "$lib/stores/sessionStore";
  import RoleIcon from "$lib/profiles/RoleIcon.svelte";
  import { getPFP, Size } from "$lib/utils/users";
  import { postAuthed } from "$lib/sorFetcher";
  import axios from "axios";

  export let user: User;

  $: isSelf = $sessionStore !== null && $sessionStore.user.id === user.id;
  let username: string = user.username;
  let usernameTaken: boolean = false;
  let bio: string = user.bio ?? "";
  let editMode: boolean = false;

  async function save() {
    if ($sessionStore === null) return;

    if (bio !== user.bio) {
      const bioUser = await postAuthed<User>(
        "/user/edit/description",
        $sessionStore.tokens,
        {
          newDescription: bio,
        }
      );
      if (bioUser.data) {
        bio = bioUser.data.bio ?? "";
        user.bio = bio;
        user = user;

        await axios.get(`/a/edit/update`);
      }
    }

    if (username.toLowerCase() === $sessionStore.user.username.toLowerCase())
      return;

    const users = await getUsernames();
    let lUsername = username.toLowerCase();
    if (users.find((x) => x.toLowerCase() === lUsername) !== undefined) {
      usernameTaken = true;
      await new Promise(function (resolve) {
        setTimeout(resolve, 3000);
      });
      usernameTaken = false;
      return;
    }

    const userResponse = await postAuthed<User>(
      "/user/edit/username",
      $sessionStore.tokens,
      {
        username,
      }
    );
    if (userResponse.data) {
      user.username = userResponse.data.username;
      user = user;

      await axios.get(`/a/edit/update`);
      if (window) {
        window.location.href = "/@" + userResponse.data.username;
      }
    }
  }

  async function edit() {
    if (editMode) await save();
    if (!usernameTaken) editMode = !editMode;
  }
</script>

<svelte:head>
  <title>Sorigin | {user.username}'s Profile</title>
  <meta name="og:image" content={getPFP(user, Size.Small)} />
  <meta name="og:title" content="{user.username}'s Profile" />
  <meta name="og:site_name" content="Sorigin" />
  <meta name="og:url" content={SORIGIN_URL} />
</svelte:head>

<section class="section">
  <div class="columns">
    <div class="column is-one-quarter">
      <div class="block">
        <figure class="image is-256x256">
          <img
            id="so-curved"
            src={getPFP(user, Size.Medium)}
            alt="User Profile"
          />
        </figure>
      </div>
      <div class="block">
        {#if isSelf}
          <button
            class="button is-dark is-fullwidth"
            on:click={edit}
            class:is-danger={username === ""}
            disabled={username === ""}
          >
            <span class="icon">
              <ion-icon name="create-outline" />
            </span>
            <span>{editMode ? "Save" : "Edit"}</span>
          </button>
        {/if}
      </div>
    </div>
    <div class="column">
      <div class="content">
        {#if isSelf && editMode && $sessionStore}
          <div class="block">
            <div class="control">
              <input
                class="input"
                type="text"
                placeholder={$sessionStore.user.username}
                class:is-danger={username === ""}
                bind:value={username}
              />
            </div>
            <p class="help is-danger">
              {usernameTaken ? "This username is taken." : ""}
            </p>
          </div>
        {:else}
          <RoleIcon {user} />
        {/if}

        {#if isSelf && editMode}
          <div class="block">
            <div class="control">
              <textarea
                class="textarea"
                placeholder="Tell us a bit about yourself..."
                bind:value={bio}
              />
            </div>
          </div>
        {:else if user.bio !== null}
          <p>{user.bio}</p>
        {:else}
          <p>
            <i
              >We don't know much about {user.username}... but we bet they're
              cool!</i
            >
          </p>
        {/if}
      </div>
    </div>
    <div class="column is-one-fifth">
      <SocialButtonGroup {user} />
    </div>
  </div>
</section>

<style>
  #so-curved {
    border-radius: 10%;
  }
</style>
