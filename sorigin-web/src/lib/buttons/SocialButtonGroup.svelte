<script lang="ts">
    import type User from '$lib/types/user'
    import ScoreSaber from '$lib/buttons/ScoreSaber.svelte'
    import Discord from '$lib/buttons/Discord.svelte'
    import { GamePlatform } from '$lib/types/user'
    import Steam from '$lib/buttons/Steam.svelte'

    export let user: User
    
</script>

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