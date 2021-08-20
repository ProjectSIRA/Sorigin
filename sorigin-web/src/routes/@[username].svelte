<script lang="ts" context="module">
    import type User from '$lib/types/user'
    import { REFETCH_URL, SORIGIN_URL } from '../utils/env'
    import { browser } from '$app/env'

    export const ssr = true

    export async function load({ page, fetch }) {
        const backend = browser ? SORIGIN_URL : REFETCH_URL
        const url = `${backend}/api/user/by-username/${page.params.username}`
        const res = await fetch(url)

        if (res.ok) {
            const user: User = await res.json()
            return { props: { user } }
        }

        return {
            status: res.status,
            error: new Error(`Could not load ${url}`)
        }
    }

</script>

<script lang="ts">
    import ScoreSaber from '$lib/buttons/ScoreSaber.svelte'
    import Discord from '$lib/buttons/Discord.svelte'
    import Steam from '$lib/buttons/Steam.svelte'
    import { GamePlatform, Role } from '$lib/types/user'
    import { getPFP, Size } from '../utils/users'
    import { authedUser } from '$lib/stores/usersStore'
    import { goto } from '$app/navigation'
    
    export let user: User

    interface RoleInfo {
        id: string
        icon: string
        name: string
    }

    $: isSelf = $authedUser !== null && $authedUser.user.id === user.id
    let bio: string = user.bio ?? ''
    let username: string = user.username
    let usernameTaken: boolean = false
    let editMode: boolean = false

    const roleData: RoleInfo | null = roleInfo()

    function roleInfo() {
        if ((user.role & Role.Owner) === Role.Owner)
            return { id: 'owner', icon: 'build', name: 'Owner of Sorigin' } as RoleInfo
        if ((user.role & Role.Admin) === Role.Admin)
            return { id: 'admin', icon: 'shield-checkmark', name: 'Sorigin Admin' } as RoleInfo
        if ((user.role & Role.Verified) === Role.Verified)
            return { id: 'verified', icon: 'checkmark-circle', name: 'Verified Account' } as RoleInfo
        return null
    }

    async function save() {
        if (bio !== user.bio) {
            const bioUpdateURL = `${browser ? SORIGIN_URL : REFETCH_URL}/api/user/edit/description`
            
            const res = await fetch(bioUpdateURL, {
                method: 'POST',
                headers: {
                    Accept: 'application/json',
                    'Content-Type': 'application/json',
                    Authorization: `Bearer ${$authedUser.token}`
                },
                body: JSON.stringify({
                    NewDescription: bio
                })
            })

            if (res.ok) {
                const body = await res.json()
                const u = body as User

                bio = u.bio
                user.bio = bio
                user = user
            }
        }

        if (username.toLowerCase() === $authedUser.user.username.toLowerCase())
            return

        const usernameListURL = `${browser ? SORIGIN_URL : REFETCH_URL}/api/user`
        const userRes = await fetch(usernameListURL, {
            headers: {
                Accept: 'application/json',
                'Content-Type': 'application/json',
            }
        })

        if (userRes.ok) {

            const userBody = await userRes.json()
            const users = userBody as string[]

            let lUsername = username.toLowerCase()
            if (users.find(x => x.toLowerCase() === lUsername) !== undefined) {
                usernameTaken = true
                await new Promise(function(resolve) {
                    setTimeout(resolve, 3000)
                });
                usernameTaken = false
                return
            }
        }
    
        await goto(`/a/edit-username/${encodeURIComponent(username)}/${$authedUser.token}`)
    }

    async function edit() {
        if (editMode)
            await save()
        if (!usernameTaken)
            editMode = !editMode
    }

</script>

<svelte:head>
    <title>Sorigin | {user.username}'s Profile</title>
    <meta name="og:title" content="{user.username}'s Profile">
    <meta name="og:image" content="{getPFP(user, Size.Small)}">
    <meta name="og:url" content={SORIGIN_URL}>
    <meta name="og:site_name" content="Sorigin">
</svelte:head>

<section class="section">
    <div class="columns">
        <div class="column is-one-quarter">
            <div class="block">
                <figure class="image is-256x256">
                    <img id="so-curved" src={getPFP(user, Size.Medium)} alt="User Profile" />
                </figure>
            </div>
            <div class="block">
                {#if isSelf}
                    <button class="button is-dark is-fullwidth" on:click={edit} class:is-danger={username === ''} disabled={username === ''}>
                    <span class="icon">
                        <ion-icon name="create-outline"></ion-icon>
                    </span>
                    <span>{editMode ? "Save" : "Edit"}</span>
                </button>
                {/if}
            </div>
        </div>
        <div class="column">
            <div class="content">
                {#if isSelf && editMode}
                    <div class="block">
                        <div class="control">
                            <input class="input" type="text" placeholder={$authedUser.user.username} class:is-danger={username === ''} bind:value={username}>
                        </div>
                        <p class="help is-danger">{usernameTaken ? 'This username is taken.' : ''}</p>
                    </div>
                {:else}
                    {#if roleData !== null}
                        <span class="icon-text has-text-info">
                            <span class="icon is-large" data-tooltip={roleData.name} id={roleData.id}>
                                <ion-icon name={roleData.icon} size="large"></ion-icon>
                            </span>
                            <span>
                                <h1 class="title">{user.username}</h1>
                            </span>
                        </span>
                    {:else}
                        <h1 class="title">{user.username}</h1>
                    {/if}
                    
                {/if}

                {#if isSelf && editMode}
                    <div class="block">
                        <div class="control">
                            <textarea class="textarea" placeholder="Tell us a bit about yourself..." bind:value={bio} />
                        </div>
                    </div>
                {:else}
                    {#if user.bio !== null}
                        <p>{user.bio}</p>
                    {:else}
                        <p><i>We don't know much about {user.username}... but we bet they're cool!</i></p>
                    {/if}
                {/if}

                
            </div>
        </div>
        <div class="column is-one-fifth">
            <div class="buttons are-large">
                {#if user.steam !== null}
                    <Steam user={user}></Steam>
                {/if}
                {#if user.discord !== null}
                    <Discord user={user}></Discord>
                {/if}
                {#if user.gamePlatform === GamePlatform.Steam}
                    <ScoreSaber user={user}></ScoreSaber>
                {/if}
            </div>
        </div>
    </div>
</section>

<style>
    #so-curved {
        border-radius: 10%;
    }

    #owner {
        color: #3AFFFF;
    }
    
    #admin {
        color: #ff3a3a;
    }
    
    #verified {
        color: #cafffc;
    }
</style>