<script lang="ts" context="module">
	import { userByID } from '$lib/sorigin';
	import type User from '$lib/types/User';

	export async function load({ page }) {
		console.log();
		const userResponse = await userByID(page.params.id);
		if (userResponse.user) {
			return { props: { user: userResponse.user } };
		}
		return {
			error: new Error(userResponse.errorMessage)
		};
	}
</script>

<script lang="ts">
	import { SORIGIN_URL } from '$lib/utils/env';

	export let user: User;

	$: countryImage = '/unknown.png';
	$: countryEmoji = 'Unknown';

	// Thieved this from Umbra >;))
	$: {
		const toTwemojiFlag = (input: string) =>
			`https://twemoji.maxcdn.com/v/13.1.0/svg/${input
				.toLowerCase()
				.match(/[a-z]/g)
				.map((i) => (i.codePointAt(0) + 127365).toString(16))
				.join('-')}.svg`;
		const toEmojiFlag = (countryCode: string): string =>
			countryCode
				.toLowerCase()
				.replace(/[a-z]/g, (i) => String.fromCodePoint((i.codePointAt(0) ?? 0) - 97 + 0x1f1e6));
		countryImage = toTwemojiFlag(user.country);
		countryEmoji = toEmojiFlag(user.country);
	}
</script>

<div class="center-screen">
	<div class="box">
		<article class="media">
			<div class="media-left">
				<figure class="image is-128x128">
					<img src={SORIGIN_URL + user.profilePicture} alt="Profile" />
				</figure>
			</div>
			<div class="media-content">
				<div class="content">
					<h2>
						<img
							style="height: 32px; display: inline-block; vertical-align: sub;"
							src={countryImage}
							alt={countryEmoji}
						/>
						{user.username}
					</h2>
				</div>
			</div>
		</article>
	</div>
</div>

<style>
	.center-screen {
		display: flex;
		justify-content: center;
		align-items: center;
		text-align: left;
		min-height: 100vh;
	}
</style>
