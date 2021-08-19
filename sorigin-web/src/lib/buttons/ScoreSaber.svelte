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

<button class="button is-primary is-fullwidth" on:click={openScoreSaber}>
    <span class="icon">
        <ion-icon name="medal"></ion-icon>
    </span>
    <span>ScoreSaber</span>
</button>

<style scoped>
    .button.is-primary {
        background-color: #F5D628;
        color: #0f1113;
    }
</style>