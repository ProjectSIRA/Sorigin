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