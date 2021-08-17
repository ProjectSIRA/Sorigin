<nav class="container-fluid">
    <ul></ul>
    <ul>
        <li>Log In</li>
    </ul>
</nav>

<slot></slot>

<style lang="css" global>
	@media only screen and (prefers-color-scheme: dark) {
		:root:not([data-theme="light"]) {
            --background-color: #222222;
		}
	}
</style>