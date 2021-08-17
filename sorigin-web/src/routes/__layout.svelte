<svelte:head>
    <meta name="title" content="Sorigin">
    <meta name="description" content="A player-unifying authorization platform for the Beat Saber community.">
    <meta name="keywords" content="beat saber, sorigin">
    <meta name="robots" content="noindex, nofollow">
    <meta http-equiv="Content-Type" content="text/html; charset=utf-8">
    <meta name="language" content="English">
</svelte:head>

<nav class="container-fluid">
    <ul>
        <li><a href="/" class="contrast">Sorigin</a></li>
    </ul>
    <ul>
        <li><a href="/login" class="secondary">Log In</a></li>
    </ul>
</nav>

<body>
    <main class="container">
        <slot></slot>  
    </main>
</body>


<style lang="css" global>
	@media only screen and (prefers-color-scheme: dark) {
		:root:not([data-theme="light"]) {
            --background-color: #0e0e0e;
            --card-background-color: #1b1b1b;
		}
	}
</style>