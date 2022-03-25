<template>
  <div>
    <NavigationBar />
    <router-view />
  </div>
</template>

<script>
import NavigationBar from "./components/NavigationBar";

export default {
  name: "App",
  components: {
    NavigationBar
  }
};
</script>

<style>
@font-face {
  font-family: "Barlow";
  src: local("Barlow"),
   url(./fonts/Barlow/Barlow-Regular.ttf) format("truetype");
}

html,
body {
  font-family: "Barlow" !important;
  text-align: center;
  margin: 0;
  min-height: 100%;
  height: 100%;
  background-color: aliceblue;
}

#app {
  font-family: "Barlow";
  width: 100%;
  margin: 0;
  padding: 0;
  min-height: 100%;
  height: 100%;
  padding-bottom: 72px;
}

#app > div {
  font-family: "Barlow";
  width: 100%;
  margin: 0;
  padding: 0;
  min-height: 100%;
  height: 100%;
}

.p-component {
  font-family: "Barlow" !important;
}
</style>
