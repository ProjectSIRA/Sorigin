<script lang="ts">
    import type User from '$lib/types/user'
    import { GamePlatform } from '$lib/types/user'

    export let user: User

    function openScoreSaber() {
        if (user.steam !== null && user.gamePlatform === GamePlatform.Steam) {
            window.open(`https://scoresaber.com/u/${user.steam.id}`, "_blank")
        }
    }

</script>

<button id="so-scoresaber-button" on:click={openScoreSaber}>ScoreSaber</button>

<style scoped>
    #so-scoresaber-button {
        --primary: #F5D628;
        --primary-hover: #917e17;
        --primary-focus: rgba(82, 71, 15, 0.25);
        --primary-inverse: rgb(15, 15, 15);
    }
</style>