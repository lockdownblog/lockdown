---
title: Docs
layout: page
---

## Quickstart

Lockdown is a static site generator. It takes text written in your favorite markup language and uses layouts to create a static website. You can tweak the site’s look and feel, <s>URLs</s>, the data displayed on the page, and more.

### Prerequisites  

Lockdown requires one of the following:  

 - .NET Core

### Instructions  

 1. Have all the [prerequisites](#prerequisites) installed  
 2. Create a new site at `my-blog`
    ```
    lockdown new my-blog
    ```
 3. Change into the new directory
    ```
    cd my-blog
    ```
 4. Build the new site and make it available locally
    ```
    lockdown run
    ```
 5. Navigate to [http://localhost:5000](http://localhost:5000)